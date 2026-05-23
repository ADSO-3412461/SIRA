using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using SIRA.Models.Entities;
using SIRA.Repositories.Interfaces;
using SIRA.ViewModels;

namespace SIRA.Controllers
{
    [Authorize]
    public class ConsolaSqlController : Controller
    {
        private readonly IAuditoriaRepository          _auditoriaRepo;
        private readonly ILogger<ConsolaSqlController> _logger;
        private readonly string                        _connectionString;

        public ConsolaSqlController(
            IAuditoriaRepository          auditoriaRepo,
            ILogger<ConsolaSqlController> logger,
            IWebHostEnvironment           env)
        {
            _auditoriaRepo    = auditoriaRepo;
            _logger           = logger;
            _connectionString = $"Data Source={Path.Combine(env.ContentRootPath, "sira.db")}";
        }

        // GET /ConsolaSql
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("EsRoot") != 1)
                return RedirectToAction("Index", "Dashboard");

            var vm = new ConsolaSqlViewModel
            {
                Auditorias = await _auditoriaRepo.ObtenerUltimasAsync(50)
            };
            return View(vm);
        }

        // POST /ConsolaSql/Ejecutar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ejecutar(string sqlQuery)
        {
            if (HttpContext.Session.GetInt32("EsRoot") != 1)
                return RedirectToAction("Index", "Dashboard");

            int    idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            string alias     = HttpContext.Session.GetString("Alias")    ?? "desconocido";

            var vm = new ConsolaSqlViewModel
            {
                SqlQuery   = sqlQuery,
                Auditorias = await _auditoriaRepo.ObtenerUltimasAsync(50)
            };

            if (string.IsNullOrWhiteSpace(sqlQuery))
            {
                vm.TieneError = true;
                vm.Mensaje    = "La consulta no puede estar vacía.";
                return View("Index", vm);
            }

            bool esSelect = sqlQuery.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase);

            try
            {
                if (esSelect)
                {
                    using var conn = new SqliteConnection(_connectionString);
                    await conn.OpenAsync();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = sqlQuery;
                    using var reader = await cmd.ExecuteReaderAsync();

                    for (int i = 0; i < reader.FieldCount; i++)
                        vm.Columnas.Add(reader.GetName(i));

                    while (await reader.ReadAsync())
                    {
                        var fila = new List<string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                            fila.Add(reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString()!);
                        vm.Filas.Add(fila);
                    }

                    vm.Mensaje = $"{vm.Filas.Count} fila(s) encontrada(s).";

                    await _auditoriaRepo.RegistrarAsync(new Auditoria
                    {
                        IdUsuario      = idUsuario,
                        Alias          = alias,
                        SqlEjecutado   = sqlQuery,
                        FechaEjecucion = DateTime.Now,
                        Resultado      = "Exitoso"
                    });
                }
                else
                {
                    using var conn = new SqliteConnection(_connectionString);
                    await conn.OpenAsync();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = sqlQuery;
                    int afectadas = await cmd.ExecuteNonQueryAsync();

                    vm.Mensaje = $"Ejecutado correctamente. Filas afectadas: {afectadas}.";

                    await _auditoriaRepo.RegistrarAsync(new Auditoria
                    {
                        IdUsuario      = idUsuario,
                        Alias          = alias,
                        SqlEjecutado   = sqlQuery,
                        FechaEjecucion = DateTime.Now,
                        Resultado      = "Exitoso"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando SQL en consola.");
                vm.TieneError = true;
                vm.Mensaje    = $"Error: {ex.Message}";

                await _auditoriaRepo.RegistrarAsync(new Auditoria
                {
                    IdUsuario      = idUsuario,
                    Alias          = alias,
                    SqlEjecutado   = sqlQuery,
                    FechaEjecucion = DateTime.Now,
                    Resultado      = "Error",
                    MensajeError   = ex.Message
                });
            }

            vm.Auditorias = await _auditoriaRepo.ObtenerUltimasAsync(50);
            return View("Index", vm);
        }
    }
}
