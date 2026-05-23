using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SIRA.Models;
using SIRA.Repositories.Interfaces;
using SIRA.Services;
using SIRA.ViewModels;

namespace SIRA.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IExcusaRepository            _excusaRepo;
        private readonly IAdministradorRepository     _administradorRepo;
        private readonly IUsuarioRepository           _usuarioRepo;
        private readonly IEmailService                _emailService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IExcusaRepository             excusaRepo,
            IAdministradorRepository      administradorRepo,
            IUsuarioRepository            usuarioRepo,
            IEmailService                 emailService,
            ILogger<DashboardController>  logger)
        {
            _excusaRepo        = excusaRepo;
            _administradorRepo = administradorRepo;
            _usuarioRepo       = usuarioRepo;
            _emailService      = emailService;
            _logger            = logger;
        }

        // GET /Dashboard
        public async Task<IActionResult> Index()
        {
            var excusas = await _excusaRepo.ObtenerTodosConEvidenciaAsync();

            var filas = excusas.Select(e => new ExcusaDashboardRow
            {
                IdExcusa           = e.IdExcusa,
                NombreEstudiante   = e.Estudiante?.NombreCompleto  ?? "—",
                TipoDocumento      = e.Estudiante?.TipoDocumento?.Sigla ?? "—",
                NumeroDocumento    = e.Estudiante?.NumeroDocumento  ?? "—",
                MotivoInasistencia = e.MotivoInasistencia ?? string.Empty,
                Estado             = string.IsNullOrEmpty(e.Estado) ? "Por revisar" : e.Estado,
                FechaRegistro      = e.FechaHoraRegistro  ?? DateTime.MinValue,
                TieneEvidencia     = e.Evidencia?.Archivo?.Length > 0
            }).ToList();

            ViewData["EsSuperUsuario"] = HttpContext.Session.GetInt32("EsSuperUsuario") == 1;

            return View(filas);
        }

        // GET /Dashboard/Descargar/{idExcusa}
        public async Task<IActionResult> Descargar(int idExcusa)
        {
            var ev = await _excusaRepo.ObtenerEvidenciaPorExcusaAsync(idExcusa);

            if (ev == null || ev.Archivo == null || ev.Archivo.Length == 0)
                return NotFound();

            var (mime, ext) = DetectarTipoArchivo(ev.Archivo);
            return File(ev.Archivo, mime, $"evidencia_{idExcusa}{ext}");
        }

        // POST /Dashboard/RegistrarDecision
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarDecision(DecisionViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Datos inválidos." });

            // Obtener el administrador desde el usuario logueado
            var idUsuarioClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idUsuarioClaim, out var idUsuario))
                return Json(new { success = false, message = "Sesión inválida." });

            var admin = await _administradorRepo.ObtenerPorUsuarioAsync(idUsuario);
            if (admin == null)
                return Json(new { success = false, message = "Administrador no encontrado." });

            // Actualizar excusa en BD
            await _excusaRepo.ActualizarDecisionAsync(
                vm.IdExcusa, vm.Estado, vm.MotivoDecision, admin.IdAdministrador);

            // Enviar correo al acudiente (fallo no bloquea la respuesta)
            try
            {
                var excusa = await _excusaRepo.ObtenerConEstudianteYAcudienteAsync(vm.IdExcusa);
                var correoAcudiente = excusa?.Estudiante?.Acudiente?.Correo;

                if (excusa != null && !string.IsNullOrEmpty(correoAcudiente))
                {
                    await _emailService.EnviarDecisionExcusaAsync(excusa, correoAcudiente);
                }
                else
                {
                    _logger.LogWarning(
                        "Correo de decisión no enviado para excusa {Id}: acudiente sin correo registrado.",
                        vm.IdExcusa);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "No se pudo enviar correo de decisión para excusa {Id}.", vm.IdExcusa);
            }

            return Json(new { success = true, estado = vm.Estado });
        }

        // GET /Dashboard/Administradores
        public async Task<IActionResult> Administradores()
        {
            if (HttpContext.Session.GetInt32("EsSuperUsuario") != 1)
                return RedirectToAction("Index");

            var admins = await _administradorRepo.ObtenerTodosAsync();
            return View(admins);
        }

        // POST /Dashboard/AgregarAdministrador
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarAdministrador(NuevoAdministradorViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Complete todos los campos correctamente." });

            // Verificar que quien llama es super admin
            if (HttpContext.Session.GetInt32("EsSuperUsuario") != 1)
                return Json(new { success = false, message = "No tiene permisos para esta acción." });

            // Verificar alias disponible
            var existente = await _usuarioRepo.ObtenerPorAliasAsync(vm.Alias);
            if (existente != null)
                return Json(new { success = false, message = "El alias de usuario ya está en uso." });

            // Crear usuario
            var nuevoUsuario = new Usuario { Alias = vm.Alias, Clave = vm.Clave, EsActivo = true };
            await _usuarioRepo.AgregarAsync(nuevoUsuario);

            var nuevoAdmin = new Administrador
            {
                IdUsuario      = nuevoUsuario.IdUsuario,
                NombreCompleto = vm.NombreCompleto,
                Correo         = vm.Correo
            };
            await _administradorRepo.AgregarAsync(nuevoAdmin);

            return Json(new { success = true });
        }

        // POST /Dashboard/ActualizarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstado(int idUsuario, bool esActivo)
        {
            if (HttpContext.Session.GetInt32("EsSuperUsuario") != 1)
                return RedirectToAction("Index");

            await _usuarioRepo.ActualizarEstadoAsync(idUsuario, esActivo);
            return RedirectToAction("Administradores");
        }

        // ── Helpers privados ─────────────────────────────────────────────────

        private static (string mime, string ext) DetectarTipoArchivo(byte[] bytes)
        {
            if (bytes.Length >= 4 && bytes[0] == 0x25 && bytes[1] == 0x50)
                return ("application/pdf", ".pdf");
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xD8)
                return ("image/jpeg", ".jpg");
            if (bytes.Length >= 4 && bytes[0] == 0x89 && bytes[1] == 0x50)
                return ("image/png", ".png");
            return ("application/octet-stream", ".bin");
        }
    }
}
