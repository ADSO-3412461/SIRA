using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIRA.Repositories.Interfaces;
using SIRA.ViewModels;

namespace SIRA.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IExcusaRepository          _excusaRepo;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IExcusaRepository            excusaRepo,
            ILogger<DashboardController> logger)
        {
            _excusaRepo = excusaRepo;
            _logger     = logger;
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

        // ── Helpers privados ─────────────────────────────────────────────────

        private static (string mime, string ext) DetectarTipoArchivo(byte[] bytes)
        {
            if (bytes.Length >= 4 && bytes[0] == 0x25 && bytes[1] == 0x50)   // %PDF
                return ("application/pdf", ".pdf");
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xD8)   // JPEG
                return ("image/jpeg", ".jpg");
            if (bytes.Length >= 4 && bytes[0] == 0x89 && bytes[1] == 0x50)   // PNG
                return ("image/png", ".png");
            return ("application/octet-stream", ".bin");
        }
    }
}
