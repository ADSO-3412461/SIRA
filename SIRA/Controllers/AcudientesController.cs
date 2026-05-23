using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIRA.Models;
using SIRA.Repositories.Interfaces;
using SIRA.ViewModels;

namespace SIRA.Controllers
{
    [Authorize]
    public class AcudientesController : Controller
    {
        private readonly IAcudienteRepository           _acudienteRepo;
        private readonly ITipoDocumentoRepository       _tipoDocRepo;
        private readonly ILogger<AcudientesController>  _logger;

        public AcudientesController(
            IAcudienteRepository          acudienteRepo,
            ITipoDocumentoRepository      tipoDocRepo,
            ILogger<AcudientesController> logger)
        {
            _acudienteRepo = acudienteRepo;
            _tipoDocRepo   = tipoDocRepo;
            _logger        = logger;
        }

        // GET /Acudientes/Crear
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var vm = new AcudienteViewModel
            {
                TiposDocumento = await BuildTiposDocumentoAsync()
            };
            return View(vm);
        }

        // POST /Acudientes/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(AcudienteViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.TiposDocumento = await BuildTiposDocumentoAsync();
                return View(vm);
            }

            // Verificar duplicados
            if (await _acudienteRepo.ExisteDocumentoAsync(vm.NumeroDocumento))
            {
                ModelState.AddModelError(nameof(vm.NumeroDocumento),
                    "Ya existe un acudiente con este número de documento.");
                vm.TiposDocumento = await BuildTiposDocumentoAsync();
                return View(vm);
            }

            if (await _acudienteRepo.ExisteCorreoAsync(vm.Correo))
            {
                ModelState.AddModelError(nameof(vm.Correo),
                    "Ya existe un acudiente registrado con este correo.");
                vm.TiposDocumento = await BuildTiposDocumentoAsync();
                return View(vm);
            }

            try
            {
                var acudiente = new Acudiente
                {
                    NombreCompleto   = vm.NombreCompleto,
                    IdTipoDocumento  = vm.IdTipoDocumento!.Value,
                    NumeroDocumento  = vm.NumeroDocumento,
                    Correo           = vm.Correo,
                    Contrasena       = vm.Contrasena
                };

                await _acudienteRepo.AgregarAsync(acudiente);
                TempData["Exito"] = "Acudiente registrado exitosamente.";
                return RedirectToAction(nameof(Crear));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar acudiente.");
                ModelState.AddModelError(string.Empty,
                    "Ocurrió un error al guardar. Intente de nuevo.");
                vm.TiposDocumento = await BuildTiposDocumentoAsync();
                return View(vm);
            }
        }

        // GET /Acudientes/Consultar
        [HttpGet]
        public async Task<IActionResult> Consultar()
        {
            var acudientes = await _acudienteRepo.ObtenerTodosAsync();
            return View(acudientes);
        }

        // GET /Acudientes/Editar/{id}
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var acudiente = await _acudienteRepo.ObtenerPorIdAsync(id);
            if (acudiente == null)
            {
                TempData["Error"] = "Acudiente no encontrado.";
                return RedirectToAction(nameof(Consultar));
            }

            var vm = new AcudienteEditarViewModel
            {
                IdAcudiente     = acudiente.IdAcudiente,
                NombreCompleto  = acudiente.NombreCompleto  ?? string.Empty,
                NumeroDocumento = acudiente.NumeroDocumento ?? string.Empty,
                Correo          = acudiente.Correo          ?? string.Empty,
                IdTipoDocumento = acudiente.IdTipoDocumento,
                TiposDocumento  = await BuildTiposDocumentoAsync()
            };

            return View(vm);
        }

        // POST /Acudientes/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(AcudienteEditarViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.TiposDocumento = await BuildTiposDocumentoAsync();
                return View(vm);
            }

            var actual = await _acudienteRepo.ObtenerPorIdAsync(vm.IdAcudiente);
            if (actual == null)
            {
                TempData["Error"] = "Acudiente no encontrado.";
                return RedirectToAction(nameof(Consultar));
            }

            if (vm.Correo != actual.Correo &&
                await _acudienteRepo.ExisteCorreoAsync(vm.Correo))
            {
                ModelState.AddModelError(nameof(vm.Correo),
                    "El correo ya está registrado por otro acudiente.");
                vm.TiposDocumento = await BuildTiposDocumentoAsync();
                return View(vm);
            }

            if (vm.NumeroDocumento != actual.NumeroDocumento &&
                await _acudienteRepo.ExisteDocumentoAsync(vm.NumeroDocumento))
            {
                ModelState.AddModelError(nameof(vm.NumeroDocumento),
                    "El número de documento ya está registrado.");
                vm.TiposDocumento = await BuildTiposDocumentoAsync();
                return View(vm);
            }

            try
            {
                await _acudienteRepo.ActualizarAsync(new Acudiente
                {
                    IdAcudiente     = vm.IdAcudiente,
                    NombreCompleto  = vm.NombreCompleto,
                    IdTipoDocumento = vm.IdTipoDocumento,
                    NumeroDocumento = vm.NumeroDocumento,
                    Correo          = vm.Correo,
                    Contrasena      = actual.Contrasena
                });

                TempData["Exito"] = "Acudiente actualizado exitosamente.";
                return RedirectToAction(nameof(Consultar));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar acudiente {Id}.", vm.IdAcudiente);
                ModelState.AddModelError(string.Empty,
                    "Ocurrió un error al guardar. Intente de nuevo.");
                vm.TiposDocumento = await BuildTiposDocumentoAsync();
                return View(vm);
            }
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
