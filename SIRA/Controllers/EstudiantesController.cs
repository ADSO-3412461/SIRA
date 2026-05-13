using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIRA.Models;
using SIRA.Repositories.Interfaces;
using SIRA.ViewModels;

namespace SIRA.Controllers
{
    [Authorize]
    public class EstudiantesController : Controller
    {
        private readonly IEstudianteRepository          _estudianteRepo;
        private readonly IAcudienteRepository           _acudienteRepo;
        private readonly ITipoDocumentoRepository       _tipoDocRepo;
        private readonly ILogger<EstudiantesController> _logger;

        public EstudiantesController(
            IEstudianteRepository           estudianteRepo,
            IAcudienteRepository            acudienteRepo,
            ITipoDocumentoRepository        tipoDocRepo,
            ILogger<EstudiantesController>  logger)
        {
            _estudianteRepo = estudianteRepo;
            _acudienteRepo  = acudienteRepo;
            _tipoDocRepo    = tipoDocRepo;
            _logger         = logger;
        }

        // GET /Estudiantes/Crear
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var vm = new EstudianteViewModel
            {
                TiposDocumento = await BuildTiposDocumentoAsync()
            };
            return View(vm);
        }

        // POST /Estudiantes/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(EstudianteViewModel vm)
        {
            // Quitar validación de campos auxiliares del buscador
            ModelState.Remove(nameof(vm.BuscarIdTipoDocumento));
            ModelState.Remove(nameof(vm.BuscarNumeroDocumento));

            if (!ModelState.IsValid)
            {
                vm.TiposDocumento = await BuildTiposDocumentoAsync();
                return View(vm);
            }

            if (await _estudianteRepo.ExisteDocumentoAsync(vm.NumeroDocumento))
            {
                ModelState.AddModelError(nameof(vm.NumeroDocumento),
                    "Ya existe un estudiante con este número de documento.");
                vm.TiposDocumento = await BuildTiposDocumentoAsync();
                return View(vm);
            }

            try
            {
                var estudiante = new Estudiante
                {
                    NombreCompleto  = vm.NombreCompleto,
                    IdTipoDocumento = vm.IdTipoDocumento!.Value,
                    NumeroDocumento = vm.NumeroDocumento,
                    IdAcudiente     = vm.IdAcudiente
                };

                await _estudianteRepo.AgregarAsync(estudiante);
                TempData["Exito"] = "Estudiante registrado exitosamente.";
                return RedirectToAction(nameof(Crear));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar estudiante.");
                ModelState.AddModelError(string.Empty,
                    "Ocurrió un error al guardar. Intente de nuevo.");
                vm.TiposDocumento = await BuildTiposDocumentoAsync();
                return View(vm);
            }
        }

        // GET /Estudiantes/Consultar
        [HttpGet]
        public async Task<IActionResult> Consultar()
        {
            var estudiantes = await _estudianteRepo.ObtenerTodosAsync();
            return View(estudiantes);
        }

        // GET /Estudiantes/BuscarAcudiente?idTipoDocumento=1&numeroDocumento=123
        [HttpGet]
        public async Task<IActionResult> BuscarAcudiente(int idTipoDocumento, string numeroDocumento)
        {
            if (idTipoDocumento <= 0 || string.IsNullOrWhiteSpace(numeroDocumento))
                return Json(new { encontrado = false });

            var acudiente = await _acudienteRepo.BuscarPorDocumentoAsync(
                idTipoDocumento, numeroDocumento.Trim());

            if (acudiente == null)
                return Json(new { encontrado = false });

            return Json(new
            {
                encontrado     = true,
                idAcudiente    = acudiente.IdAcudiente,
                nombreCompleto = acudiente.NombreCompleto,
                correo         = acudiente.Correo
            });
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private async Task<List<SelectListItem>> BuildTiposDocumentoAsync()
        {
            var tipos = await _tipoDocRepo.ObtenerTodosAsync();

            var items = tipos.Select(t => new SelectListItem
            {
                Value = t.IdTipoDocumento.ToString(),
                Text  = $"{t.Sigla} — {t.Descripcion}"
            }).ToList();

            items.Insert(0, new SelectListItem
            {
                Value    = "",
                Text     = "-- Seleccione tipo de documento --",
                Disabled = true,
                Selected = true
            });

            return items;
        }
    }
}
