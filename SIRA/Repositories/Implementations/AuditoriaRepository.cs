using Microsoft.EntityFrameworkCore;
using SIRA.Data;
using SIRA.Models.Entities;
using SIRA.Repositories.Interfaces;

namespace SIRA.Repositories.Implementations
{
    public class AuditoriaRepository : IAuditoriaRepository
    {
        private readonly AppDbContext _context;

        public AuditoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task RegistrarAsync(Auditoria auditoria)
        {
            await _context.Auditorias.AddAsync(auditoria);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Auditoria>> ObtenerUltimasAsync(int cantidad = 50)
        {
            return await _context.Auditorias
                .OrderByDescending(a => a.FechaEjecucion)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<(List<Auditoria> Registros, int TotalRegistros)> ObtenerPaginadoAsync(
            int pagina, int registrosPorPagina)
        {
            var query = _context.Auditorias
                .OrderByDescending(a => a.FechaEjecucion);

            int total = await query.CountAsync();

            var registros = await query
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            return (registros, total);
        }
    }
}
