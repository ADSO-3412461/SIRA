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
        private readonly IEstudianteRepository           _estudianteRepo;
        private readonly IAcudienteRepository            _acudienteRepo;
        private readonly ITipoDocumentoRepository        _tipoDocRepo;
        private readonly IInstitucionEducativaRepository _institucionRepo;
        private readonly ILogger<EstudiantesController>  _logger;

        public EstudiantesController(
            IEstudianteRepository            estudianteRepo,
            IAcudienteRepository             acudienteRepo,
            ITipoDocumentoRepository         tipoDocRepo,
            IInstitucionEducativaRepository  institucionRepo,
            ILogger<EstudiantesController>   logger)
        {
            _estudianteRepo  = estudianteRepo;
            _acudienteRepo   = acudienteRepo;
            _tipoDocRepo     = tipoDocRepo;
            _institucionRepo = institucionRepo;
            _logger          = logger;
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
            int  idInstitucion  = HttpContext.Session.GetInt32("IdInstitucion")  ?? 0;
            bool esSuperUsuario = HttpContext.Session.GetInt32("EsSuperUsuario") == 1
                               || HttpContext.Session.GetInt32("EsRoot")         == 1;

            var estudiantes = await _estudianteRepo.ObtenerTodosAsync(idInstitucion, esSuperUsuario);
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

        // GET /Estudiantes/Editar/{id}
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            if (HttpContext.Session.GetInt32("EsSuperUsuario") != 1)
                return RedirectToAction("Index", "Dashboard");

            var estudiante = await _estudianteRepo.ObtenerPorIdAsync(id);
            if (estudiante == null)
            {
                TempData["Error"] = "Estudiante no encontrado.";
                return RedirectToAction(nameof(Consultar));
            }

            var vm = new EstudianteEditarViewModel
            {
                IdEstudiante           = estudiante.IdEstudiante,
                NombreCompleto         = estudiante.NombreCompleto ?? string.Empty,
                NumeroDocumento        = estudiante.NumeroDocumento ?? string.Empty,
                IdTipoDocumento        = estudiante.IdTipoDocumento,
                IdAcudiente            = estudiante.IdAcudiente,
                IdInstitucionEducativa = estudiante.IdInstitucionEducativa,
                TiposDocumento         = await BuildTiposDocumentoAsync(),
                Acudientes             = await BuildAcudientesAsync(),
                InstitucionesEducativas = await BuildInstitucionesAsync()
            };

            return View(vm);
        }

        // POST /Estudiantes/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EstudianteEditarViewModel vm)
        {
            if (HttpContext.Session.GetInt32("EsSuperUsuario") != 1)
                return RedirectToAction("Index", "Dashboard");

            if (!ModelState.IsValid)
            {
                vm.TiposDocumento          = await BuildTiposDocumentoAsync();
                vm.Acudientes              = await BuildAcudientesAsync();
                vm.InstitucionesEducativas = await BuildInstitucionesAsync();
                return View(vm);
            }

            if (vm.IdInstitucionEducativa > 0)
            {
                var todas = await _institucionRepo.ObtenerParaDropdownAsync();
                var inst = todas.FirstOrDefault(i => i.IdInstitucionEducativa == vm.IdInstitucionEducativa.Value);
                if (inst != null && !inst.EsActivo)
                {
                    ModelState.AddModelError(nameof(vm.IdInstitucionEducativa),
                        "La institución seleccionada está inactiva. Seleccione una institución activa.");
                    vm.TiposDocumento          = await BuildTiposDocumentoAsync();
                    vm.Acudientes              = await BuildAcudientesAsync();
                    vm.InstitucionesEducativas = await BuildInstitucionesAsync();
                    return View(vm);
                }
            }

            try
            {
                var actualizado = new Estudiante
                {
                    IdEstudiante           = vm.IdEstudiante,
                    NombreCompleto         = vm.NombreCompleto,
                    NumeroDocumento        = vm.NumeroDocumento,
                    IdTipoDocumento        = vm.IdTipoDocumento!.Value,
                    IdAcudiente            = vm.IdAcudiente,
                    IdInstitucionEducativa = vm.IdInstitucionEducativa
                };

                await _estudianteRepo.ActualizarAsync(actualizado);
                TempData["Exito"] = "Estudiante actualizado exitosamente.";
                return RedirectToAction(nameof(Consultar));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estudiante {Id}.", vm.IdEstudiante);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar. Intente de nuevo.");
                vm.TiposDocumento          = await BuildTiposDocumentoAsync();
                vm.Acudientes              = await BuildAcudientesAsync();
                vm.InstitucionesEducativas = await BuildInstitucionesAsync();
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

        private async Task<List<SelectListItem>> BuildAcudientesAsync()
        {
            int  idInstitucion  = HttpContext.Session.GetInt32("IdInstitucion")  ?? 0;
            bool esSuperUsuario = HttpContext.Session.GetInt32("EsSuperUsuario") == 1
                               || HttpContext.Session.GetInt32("EsRoot")         == 1;

            var acudientes = await _acudienteRepo.ObtenerTodosAsync(idInstitucion, esSuperUsuario);

            var items = acudientes.OrderBy(a => a.NombreCompleto).Select(a => new SelectListItem
            {
                Value = a.IdAcudiente.ToString(),
                Text  = a.NombreCompleto ?? "—"
            }).ToList();

            items.Insert(0, new SelectListItem
            {
                Value = "",
                Text  = "-- Sin acudiente --"
            });

            return items;
        }

        private async Task<List<SelectListItem>> BuildInstitucionesAsync()
        {
            var instituciones = await _institucionRepo.ObtenerParaDropdownAsync();

            var items = instituciones
                .Select(i => new SelectListItem
                {
                    Value = i.IdInstitucionEducativa.ToString(),
                    Text  = i.EsActivo
                        ? (i.NombreInstitucion ?? "—")
                        : $"{i.NombreInstitucion} (Inactiva)"
                }).ToList();

            items.Insert(0, new SelectListItem
            {
                Value = "",
                Text  = "-- Sin institución --"
            });

            return items;
        }
    }
}
