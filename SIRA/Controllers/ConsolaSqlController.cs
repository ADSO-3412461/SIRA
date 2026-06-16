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

        private const int RegistrosPorPagina = 10;

        public ConsolaSqlController(
            IAuditoriaRepository          auditoriaRepo,
            ILogger<ConsolaSqlController> logger,
            IConfiguration                configuration)
        {
            _auditoriaRepo    = auditoriaRepo;
            _logger           = logger;
            _connectionString = configuration.GetConnectionString("SiraDb")!;
        }

        // GET /ConsolaSql
        [HttpGet]
        public async Task<IActionResult> Index(int pagina = 1)
        {
            if (HttpContext.Session.GetInt32("EsRoot") != 1)
                return RedirectToAction("Index", "Dashboard");

            var (registros, total) = await _auditoriaRepo.ObtenerPaginadoAsync(pagina, RegistrosPorPagina);

            int totalPaginas = (int)Math.Ceiling((double)total / RegistrosPorPagina);
            if (totalPaginas < 1) totalPaginas = 1;
            if (pagina < 1)            pagina = 1;
            if (pagina > totalPaginas) pagina = totalPaginas;

            var vm = new ConsolaSqlViewModel
            {
                Auditorias     = registros,
                PaginaActual   = pagina,
                TotalPaginas   = totalPaginas,
                TotalRegistros = total
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

            var (registros, total) = await _auditoriaRepo.ObtenerPaginadoAsync(1, RegistrosPorPagina);
            int totalPaginas = (int)Math.Ceiling((double)total / RegistrosPorPagina);
            if (totalPaginas < 1) totalPaginas = 1;

            var vm = new ConsolaSqlViewModel
            {
                SqlQuery       = sqlQuery,
                Auditorias     = registros,
                PaginaActual   = 1,
                TotalPaginas   = totalPaginas,
                TotalRegistros = total
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

            // Recargar historial paginado en página 1 tras ejecutar
            var (reg2, total2) = await _auditoriaRepo.ObtenerPaginadoAsync(1, RegistrosPorPagina);
            int tp2 = (int)Math.Ceiling((double)total2 / RegistrosPorPagina);
            if (tp2 < 1) tp2 = 1;
            vm.Auditorias     = reg2;
            vm.TotalRegistros = total2;
            vm.TotalPaginas   = tp2;
            vm.PaginaActual   = 1;

            return View("Index", vm);
        }
    }
}
