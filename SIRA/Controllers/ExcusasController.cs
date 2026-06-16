using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIRA.Models;
using SIRA.Repositories.Interfaces;
using SIRA.Services;
using SIRA.ViewModels;

namespace SIRA.Controllers
{
    public class ExcusasController : Controller
    {
        private readonly IExcusaRepository                   _excusaRepo;
        private readonly IEstudianteRepository               _estudianteRepo;
        private readonly IAdministradorRepository            _administradorRepo;
        private readonly IInstitucionEducativaRepository     _institucionRepo;
        private readonly IEmailService                       _emailService;
        private readonly IWebHostEnvironment                 _env;
        private readonly ILogger<ExcusasController>          _logger;

        public ExcusasController(
            IExcusaRepository                   excusaRepo,
            IEstudianteRepository               estudianteRepo,
            IAdministradorRepository            administradorRepo,
            IInstitucionEducativaRepository     institucionRepo,
            IEmailService                       emailService,
            IWebHostEnvironment                 env,
            ILogger<ExcusasController>          logger)
        {
            _excusaRepo        = excusaRepo;
            _estudianteRepo    = estudianteRepo;
            _administradorRepo = administradorRepo;
            _institucionRepo   = institucionRepo;
            _emailService      = emailService;
            _env               = env;
            _logger            = logger;
        }

        // GET /Excusas/Crear?idInstitucion={id}
        [HttpGet]
        public async Task<IActionResult> Crear(int idInstitucion)
        {
            try
            {
                if (idInstitucion <= 0)
                {
                    TempData["Error"] = "La institución seleccionada no es válida. Por favor seleccione otra.";
                    return RedirectToAction("Instituciones", "Home");
                }

                var institucion = await _institucionRepo.ObtenerPorIdAsync(idInstitucion);
                if (institucion == null)
                {
                    TempData["Error"] = "La institución seleccionada no existe. Por favor seleccione otra.";
                    return RedirectToAction("Instituciones", "Home");
                }

                if (!institucion.EsActivo)
                {
                    TempData["Error"] = "La institución seleccionada no está disponible. Por favor seleccione otra.";
                    return RedirectToAction("Instituciones", "Home");
                }

                var estudiantes = await BuildStudentListAsync(idInstitucion);
                if (estudiantes == null || estudiantes.Count <= 1)
                {
                    TempData["Advertencia"] = "La institución aún no tiene estudiantes registrados. Por favor contacte al administrador.";
                    return RedirectToAction("Instituciones", "Home");
                }

                ViewBag.IdInstitucion = idInstitucion;
                return View(new ExcusaViewModel { Estudiantes = estudiantes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al cargar el formulario de excusa para institución {Id}.", idInstitucion);
                TempData["Error"] = "Ocurrió un error inesperado. Por favor intente nuevamente.";
                return RedirectToAction("Instituciones", "Home");
            }
        }

        // POST /Excusas/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ExcusaViewModel vm, int idInstitucion)
        {
            // ── Verificar que idInstitucion sea válido ────────────────────────
            if (idInstitucion <= 0)
            {
                TempData["Error"] = "No se pudo identificar la institución.";
                return RedirectToAction("Instituciones", "Home");
            }

            // ── Verificar que la institución sigue activa ─────────────────────
            var institucion = await _institucionRepo.ObtenerPorIdAsync(idInstitucion);
            if (institucion == null || !institucion.EsActivo)
            {
                TempData["Error"] = "La institución seleccionada ya no está disponible.";
                return RedirectToAction("Instituciones", "Home");
            }

            // ── Validar que se adjuntó evidencia ──────────────────────────────
            if (vm.Evidencia == null || vm.Evidencia.Length == 0)
            {
                ModelState.AddModelError(nameof(vm.Evidencia),
                    "Debe adjuntar un documento de evidencia.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.IdInstitucion = idInstitucion;
                vm.Estudiantes = await BuildStudentListAsync(idInstitucion);
                return View(vm);
            }

            // ── Validar formato y tamaño del archivo ──────────────────────────
            var allowedMime = new[] { "image/jpeg", "image/png", "application/pdf" };
            if (!allowedMime.Contains(vm.Evidencia!.ContentType))
            {
                ModelState.AddModelError(nameof(vm.Evidencia),
                    "Solo se permiten archivos JPG, PNG o PDF.");
                ViewBag.IdInstitucion = idInstitucion;
                vm.Estudiantes = await BuildStudentListAsync(idInstitucion);
                return View(vm);
            }

            const long maxBytes = 5L * 1024 * 1024;
            if (vm.Evidencia.Length > maxBytes)
            {
                ModelState.AddModelError(nameof(vm.Evidencia),
                    "El archivo no puede superar los 5 MB.");
                ViewBag.IdInstitucion = idInstitucion;
                vm.Estudiantes = await BuildStudentListAsync(idInstitucion);
                return View(vm);
            }

            // ── Leer bytes del archivo ────────────────────────────────────────
            using var ms = new MemoryStream();
            await vm.Evidencia.CopyToAsync(ms);
            var archivoBytes  = ms.ToArray();
            var archivoNombre = Path.GetFileName(vm.Evidencia.FileName);
            var archivoMime   = vm.Evidencia.ContentType;

            // ── Guardar excusa ────────────────────────────────────────────────
            var excusa = new Excusa
            {
                IdEstudiante              = vm.IdEstudiante!.Value,
                MotivoInasistencia        = vm.MotivoInasistencia,
                Estado                    = "Por revisar",
                FechaHoraRegistro         = DateTime.Now,
                IdInstitucionEducativa    = idInstitucion
            };

            try
            {
                await _excusaRepo.AgregarAsync(excusa);

                // ── Guardar BLOB en evidencia_excusa ──────────────────────────
                await _excusaRepo.AgregarEvidenciaAsync(new EvidenciaExcusa
                {
                    IdExcusa = excusa.IdExcusa,
                    Archivo  = archivoBytes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al guardar excusa para el estudiante con id {Id}",
                    vm.IdEstudiante);

                TempData["Error"] = _env.IsDevelopment()
                    ? $"Error: {ex.Message}"
                    : "Ocurrió un error al guardar la excusa. Intente de nuevo.";

                ViewBag.IdInstitucion = idInstitucion;
                vm.Estudiantes = await BuildStudentListAsync(idInstitucion);
                return View(vm);
            }

            // ── Enviar correo (fallo no impide la confirmación al usuario) ────
            // Regla: primero al admin de la institución; si falla, fallback a super usuarios.
            var estudiante  = await _estudianteRepo.ObtenerPorIdAsync(excusa.IdEstudiante);
            var correoAdmin = institucion.Administrador?.Correo;
            bool enviadoAdmin = false;

            if (estudiante != null && !string.IsNullOrWhiteSpace(correoAdmin))
            {
                try
                {
                    await _emailService.EnviarNotificacionExcusaAsync(
                        estudiante, excusa, archivoBytes, archivoNombre, archivoMime, correoAdmin);
                    enviadoAdmin = true;
                    _logger.LogInformation(
                        "Correo de excusa {IdExcusa} enviado al admin de la institución ({Correo}).",
                        excusa.IdExcusa, correoAdmin);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Falló envío al admin de la institución ({Correo}) para excusa {IdExcusa}. Se intentará fallback a super usuarios.",
                        correoAdmin, excusa.IdExcusa);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Excusa {IdExcusa}: admin de la institución sin correo registrado. Se intentará fallback a super usuarios.",
                    excusa.IdExcusa);
            }

            // Fallback: enviar a super usuarios si no se logró enviar al admin
            if (!enviadoAdmin && estudiante != null)
            {
                var correosSU = await _administradorRepo.ObtenerCorreosSuperUsuariosAsync();
                if (correosSU.Count == 0)
                {
                    _logger.LogWarning(
                        "Excusa {IdExcusa}: no hay super usuarios con correo registrado para fallback.",
                        excusa.IdExcusa);
                }
                else
                {
                    foreach (var correoSU in correosSU)
                    {
                        try
                        {
                            await _emailService.EnviarNotificacionExcusaAsync(
                                estudiante, excusa, archivoBytes, archivoNombre, archivoMime, correoSU);
                            _logger.LogInformation(
                                "Correo de excusa {IdExcusa} enviado a super usuario ({Correo}) por fallback.",
                                excusa.IdExcusa, correoSU);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex,
                                "Falló envío de fallback a super usuario ({Correo}) para excusa {IdExcusa}.",
                                correoSU, excusa.IdExcusa);
                        }
                    }
                }
            }

            TempData["Exito"] = "La excusa fue registrada exitosamente.";
            return RedirectToAction(nameof(Crear), new { idInstitucion });
        }

        // POST /Excusas/ActualizarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstado(int idExcusa, string estado, string motivoDecision)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToAction("Index", "Dashboard");

            var excusaExistente = await _excusaRepo.ObtenerPorIdAsync(idExcusa);
            if (excusaExistente == null)
            {
                TempData["Error"] = "La excusa no fue encontrada.";
                return RedirectToAction("Index", "Dashboard");
            }

            if (excusaExistente.Estado == "Aprobada" || excusaExistente.Estado == "Rechazada")
            {
                TempData["Error"] = "Esta excusa ya fue procesada y no puede modificarse.";
                return RedirectToAction("Index", "Dashboard");
            }

            if (estado != "Aprobada" && estado != "Rechazada")
            {
                TempData["Error"] = "El estado proporcionado no es válido.";
                return RedirectToAction("Index", "Dashboard");
            }

            // Obtener admin desde el usuario logueado
            var idUsuarioClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idUsuarioClaim, out var idUsuario))
            {
                TempData["Error"] = "Sesión inválida.";
                return RedirectToAction("Index", "Dashboard");
            }

            var admin = await _administradorRepo.ObtenerPorUsuarioAsync(idUsuario);
            if (admin == null)
            {
                TempData["Error"] = "Administrador no encontrado.";
                return RedirectToAction("Index", "Dashboard");
            }

            // Actualizar registrando quién tomó la decisión
            await _excusaRepo.ActualizarDecisionAsync(idExcusa, estado, motivoDecision, admin.IdAdministrador);

            // Enviar correo al acudiente (fallo no bloquea la respuesta)
            try
            {
                var excusaConDatos = await _excusaRepo.ObtenerConEstudianteYAcudienteAsync(idExcusa);
                var correoAcudiente = excusaConDatos?.Estudiante?.Acudiente?.Correo;

                if (excusaConDatos != null && !string.IsNullOrEmpty(correoAcudiente))
                {
                    await _emailService.EnviarDecisionExcusaAsync(excusaConDatos, correoAcudiente);
                }
                else
                {
                    _logger.LogWarning(
                        "Correo de decisión no enviado para excusa {Id}: acudiente sin correo registrado.",
                        idExcusa);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "No se pudo enviar correo de decisión para excusa {Id}.", idExcusa);
            }

            TempData["Exito"] = estado == "Aprobada"
                ? "La excusa fue aprobada correctamente."
                : "La excusa fue rechazada correctamente.";

            return RedirectToAction("Index", "Dashboard");
        }

        // ── Helpers privados ──────────────────────────────────────────────────

        private async Task<List<SelectListItem>> BuildStudentListAsync(int idInstitucion)
        {
            var students = await _estudianteRepo.ObtenerPorInstitucionAsync(idInstitucion);

            var items = students.Select(s => new SelectListItem
            {
                Value = s.IdEstudiante.ToString(),
                Text  = $"{s.NombreCompleto} — {s.TipoDocumento?.Sigla ?? "Doc."} {s.NumeroDocumento}"
            }).ToList();

            items.Insert(0, new SelectListItem
            {
                Value    = "",
                Text     = "-- Seleccione un estudiante --",
                Disabled = true,
                Selected = true
            });

            return items;
        }
    }
}
