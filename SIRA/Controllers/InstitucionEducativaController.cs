using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIRA.Models;
using SIRA.Repositories.Interfaces;
using SIRA.ViewModels;

namespace SIRA.Controllers
{
    [Authorize]
    public class InstitucionEducativaController : Controller
    {
        private readonly IInstitucionEducativaRepository _repo;
        private readonly ILogger<InstitucionEducativaController> _logger;

        public InstitucionEducativaController(
            IInstitucionEducativaRepository repo,
            ILogger<InstitucionEducativaController> logger)
        {
            _repo   = repo;
            _logger = logger;
        }

        // GET /InstitucionEducativa
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("EsSuperUsuario") != 1)
                return RedirectToAction("Index", "Home");

            var instituciones = await _repo.ObtenerTodosAsync();
            return View(instituciones);
        }

        // GET /InstitucionEducativa/Crear
        public async Task<IActionResult> Crear()
        {
            if (HttpContext.Session.GetInt32("EsSuperUsuario") != 1)
                return RedirectToAction("Index", "Home");

            ViewBag.Administradores = await _repo.ObtenerAdministradoresDisponiblesAsync();
            return View();
        }

        // POST /InstitucionEducativa/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(InstitucionEducativaViewModel vm)
        {
            if (HttpContext.Session.GetInt32("EsSuperUsuario") != 1)
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                ViewBag.Administradores = await _repo.ObtenerAdministradoresDisponiblesAsync();
                return View(vm);
            }

            if (await _repo.ExisteAdministradorAsync(vm.IdAdministrador))
            {
                ModelState.AddModelError(nameof(vm.IdAdministrador),
                    "Este administrador ya tiene una institución asignada.");
                ViewBag.Administradores = await _repo.ObtenerAdministradoresDisponiblesAsync();
                return View(vm);
            }

            var nueva = new InstitucionEducativa
            {
                NombreInstitucion = vm.NombreInstitucion,
                Direccion         = vm.Direccion,
                Telefono          = vm.Telefono,
                IdAdministrador   = vm.IdAdministrador,
                EsActivo          = true
            };

            await _repo.AgregarAsync(nueva);

            _logger.LogInformation("Institución educativa '{Nombre}' registrada.", vm.NombreInstitucion);
            TempData["Exito"] = "Institución educativa registrada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET /InstitucionEducativa/Editar/{id}
        public async Task<IActionResult> Editar(int id)
        {
            if (HttpContext.Session.GetInt32("EsSuperUsuario") != 1)
                return RedirectToAction("Index", "Home");

            var institucion = await _repo.ObtenerPorIdAsync(id);
            if (institucion == null)
                return NotFound();

            // Admins disponibles (sin institución asignada) + el actual (aunque ya tenga asignación)
            var disponibles = (await _repo.ObtenerAdministradoresDisponiblesAsync()).ToList();
            if (institucion.Administrador != null &&
                !disponibles.Any(a => a.IdAdministrador == institucion.IdAdministrador))
            {
                disponibles.Insert(0, institucion.Administrador);
            }
            ViewBag.Administradores = disponibles;

            var vm = new InstitucionEducativaViewModel
            {
                IdInstitucionEducativa = institucion.IdInstitucionEducativa,
                NombreInstitucion      = institucion.NombreInstitucion ?? string.Empty,
                Direccion              = institucion.Direccion         ?? string.Empty,
                Telefono               = institucion.Telefono          ?? string.Empty,
                IdAdministrador        = institucion.IdAdministrador,
                EsActivo               = institucion.EsActivo
            };

            return View(vm);
        }

        // POST /InstitucionEducativa/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(InstitucionEducativaViewModel vm)
        {
            if (HttpContext.Session.GetInt32("EsSuperUsuario") != 1)
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                var disponibles = (await _repo.ObtenerAdministradoresDisponiblesAsync()).ToList();
                var actual = await _repo.ObtenerPorIdAsync(vm.IdInstitucionEducativa);
                if (actual?.Administrador != null &&
                    !disponibles.Any(a => a.IdAdministrador == actual.IdAdministrador))
                {
                    disponibles.Insert(0, actual.Administrador);
                }
                ViewBag.Administradores = disponibles;
                return View(vm);
            }

            // Verificar conflicto de administrador solo si cambió
            var original = await _repo.ObtenerPorIdAsync(vm.IdInstitucionEducativa);
            if (original == null)
                return NotFound();

            if (vm.IdAdministrador != original.IdAdministrador &&
                await _repo.ExisteAdministradorAsync(vm.IdAdministrador))
            {
                ModelState.AddModelError(nameof(vm.IdAdministrador),
                    "Este administrador ya tiene una institución asignada.");
                var disponibles = (await _repo.ObtenerAdministradoresDisponiblesAsync()).ToList();
                if (original.Administrador != null &&
                    !disponibles.Any(a => a.IdAdministrador == original.IdAdministrador))
                {
                    disponibles.Insert(0, original.Administrador);
                }
                ViewBag.Administradores = disponibles;
                return View(vm);
            }

            var actualizado = new InstitucionEducativa
            {
                IdInstitucionEducativa = vm.IdInstitucionEducativa,
                NombreInstitucion      = vm.NombreInstitucion,
                Direccion              = vm.Direccion,
                Telefono               = vm.Telefono,
                IdAdministrador        = vm.IdAdministrador
            };

            await _repo.ActualizarAsync(actualizado);

            _logger.LogInformation("Institución educativa '{Nombre}' actualizada.", vm.NombreInstitucion);
            TempData["Exito"] = "Institución educativa actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST /InstitucionEducativa/ActualizarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstado(int id, bool esActivo)
        {
            if (HttpContext.Session.GetInt32("EsSuperUsuario") != 1)
                return RedirectToAction("Index", "Home");

            await _repo.ActualizarEstadoAsync(id, esActivo);

            TempData["Exito"] = esActivo
                ? "Institución activada correctamente."
                : "Institución desactivada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
