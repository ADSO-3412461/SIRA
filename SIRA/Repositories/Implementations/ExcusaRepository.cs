using Microsoft.EntityFrameworkCore;
using SIRA.Data;
using SIRA.Models;
using SIRA.Repositories.Interfaces;

namespace SIRA.Repositories.Implementations
{
    public class ExcusaRepository : IExcusaRepository
    {
        private readonly AppDbContext _context;

        public ExcusaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AgregarAsync(Excusa excusa)
        {
            await _context.Excusas.AddAsync(excusa);
            await _context.SaveChangesAsync();
        }

        public async Task AgregarEvidenciaAsync(EvidenciaExcusa evidencia)
        {
            await _context.EvidenciasExcusa.AddAsync(evidencia);
            await _context.SaveChangesAsync();
        }

        public async Task<Excusa?> ObtenerPorIdAsync(int id)
        {
            return await _context.Excusas
                .Include(e => e.Estudiante)
                .FirstOrDefaultAsync(e => e.IdExcusa == id);
        }

        public async Task<IEnumerable<Excusa>> ObtenerTodosAsync()
        {
            return await _context.Excusas
                .Include(e => e.Estudiante)
                .OrderByDescending(e => e.FechaHoraRegistro)
                .ToListAsync();
        }

        public async Task<IEnumerable<Excusa>> ObtenerTodosConEvidenciaAsync()
        {
            Excusa excusa = new Excusa();

            try
            {
                return await _context.Excusas
                     .Include(e => e.Estudiante)
                     .ThenInclude(est => est!.TipoDocumento)
                     .Include(e => e.Evidencia)
                     .OrderByDescending(e => e.FechaHoraRegistro)
                     .ToListAsync();
            }
            catch (Exception ex)
            {
                // Manejar la excepción, por ejemplo, registrándola o lanzándola nuevamente
                Console.WriteLine($"Error al obtener excusas con evidencia: {ex.Message}");
                throw; // O puedes optar por manejarla de otra manera según tu lógica de negocio

                //return (IEnumerable<Excusa>)excusa;
            }

         
        }

        public async Task<EvidenciaExcusa?> ObtenerEvidenciaPorExcusaAsync(int idExcusa)
        {
            return await _context.EvidenciasExcusa
                .FirstOrDefaultAsync(ev => ev.IdExcusa == idExcusa);
        }
    }
}
