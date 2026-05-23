using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SIRA.Repositories.Interfaces;
using SIRA.ViewModels;

namespace SIRA.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUsuarioRepository usuarioRepo, ILogger<AuthController> logger)
        {
            _usuarioRepo = usuarioRepo;
            _logger      = logger;
        }

        // GET /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        // POST /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var usuario = await _usuarioRepo.ObtenerPorAliasAsync(vm.Alias);

            if (usuario == null || usuario.Clave != vm.Clave)
            {
                ModelState.AddModelError(string.Empty, "Alias o clave incorrectos.");
                return View(vm);
            }

            if (!usuario.EsActivo)
            {
                ModelState.AddModelError(string.Empty, "Usuario inactivo. Contacte al administrador.");
                return View(vm);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name,           usuario.Alias)
            };

            var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = false });

            HttpContext.Session.SetInt32("EsSuperUsuario", usuario.EsSuperUsuario ? 1 : 0);
            HttpContext.Session.SetInt32("EsRoot",         usuario.EsRoot         ? 1 : 0);
            HttpContext.Session.SetInt32("IdUsuario",      usuario.IdUsuario);
            HttpContext.Session.SetString("Alias",         usuario.Alias);

            _logger.LogInformation("Usuario {Alias} inició sesión.", usuario.Alias);
            return RedirectToAction("Index", "Dashboard");
        }

        // GET /Auth/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
