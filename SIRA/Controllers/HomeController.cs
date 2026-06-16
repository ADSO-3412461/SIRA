using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SIRA.Models;
using SIRA.Repositories.Interfaces;

namespace SIRA.Controllers
{
    public class HomeController : Controller
    {
        private readonly IInstitucionEducativaRepository _institucionRepo;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IInstitucionEducativaRepository institucionRepo,
            ILogger<HomeController> logger)
        {
            _institucionRepo = institucionRepo;
            _logger          = logger;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        // GET /Home/Instituciones
        public async Task<IActionResult> Instituciones()
        {
            var instituciones = await _institucionRepo.ObtenerTodosAsync();
            return View(instituciones);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
