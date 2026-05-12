using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIRA.Models;
using SIRA.Repositories.Interfaces;
using SIRA.ViewModels;

namespace SIRA.Controllers
{
    public class ExcusasController : Controller
    {
        private readonly IExcusaRepository      _excusaRepo;
        private readonly IEstudianteRepository  _estudianteRepo;
        private readonly IWebHostEnvironment    _env;
        private readonly ILogger<ExcusasController> _logger;

        public ExcusasController(
            IExcusaRepository     excusaRepo,
            IEstudianteRepository estudianteRepo,
            IWebHostEnvironment   env,
            ILogger<ExcusasController> logger)
        {
            _excusaRepo     = excusaRepo;
            _estudianteRepo = estudianteRepo;
            _env            = env;
            _logger         = logger;
        }

        // GET /Excusas/Crear
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var vm = new ExcusaViewModel
            {
                Estudiantes = await BuildStudentListAsync()
            };
            return View(vm);
        }

        // POST /Excusas/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ExcusaViewModel vm)
        {
            // ── Validar que se adjuntó evidencia ──────────────────────────────
            if (vm.Evidencia == null || vm.Evidencia.Length == 0)
            {
                ModelState.AddModelError(nameof(vm.Evidencia),
                    "Debe adjuntar un documento de evidencia.");
            }

            if (!ModelState.IsValid)
            {
                vm.Estudiantes = await BuildStudentListAsync();
                return View(vm);
            }

            // ── Validar formato y tamaño del archivo ──────────────────────────
            var allowedMime = new[] { "image/jpeg", "image/png", "application/pdf" };
            if (!allowedMime.Contains(vm.Evidencia!.ContentType))
            {
                ModelState.AddModelError(nameof(vm.Evidencia),
                    "Solo se permiten archivos JPG, PNG o PDF.");
                vm.Estudiantes = await BuildStudentListAsync();
                return View(vm);
            }

            const long maxBytes = 5L * 1024 * 1024;
            if (vm.Evidencia.Length > maxBytes)
            {
                ModelState.AddModelError(nameof(vm.Evidencia),
                    "El archivo no puede superar los 5 MB.");
                vm.Estudiantes = await BuildStudentListAsync();
                return View(vm);
            }

            // ── Leer bytes del archivo ────────────────────────────────────────
            using var ms = new MemoryStream();
            await vm.Evidencia.CopyToAsync(ms);
            var archivoBytes = ms.ToArray();

            // ── Guardar excusa ────────────────────────────────────────────────
            var excusa = new Excusa
            {
                IdEstudiante       = vm.IdEstudiante!.Value,
                MotivoInasistencia = vm.MotivoInasistencia,
                Estado             = "Por revisar",
                FechaHoraRegistro  = DateTime.Now
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

                TempData["Exito"] = "La excusa fue registrada exitosamente.";
                return RedirectToAction(nameof(Crear));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al guardar excusa para el estudiante con id {Id}",
                    vm.IdEstudiante);

                // En desarrollo se muestra el error real para facilitar el diagnóstico
                TempData["Error"] = _env.IsDevelopment()
                    ? $"Error: {ex.Message}"
                    : "Ocurrió un error al guardar la excusa. Intente de nuevo.";

                vm.Estudiantes = await BuildStudentListAsync();
                return View(vm);
            }
        }

        // ── Helpers privados ──────────────────────────────────────────────────

        private async Task<List<SelectListItem>> BuildStudentListAsync()
        {
            var students = await _estudianteRepo.ObtenerTodosAsync();

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
