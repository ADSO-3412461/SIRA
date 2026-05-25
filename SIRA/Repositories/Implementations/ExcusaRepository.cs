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

        public async Task<IEnumerable<Excusa>> ObtenerTodosAsync(int idInstitucion, bool esSuperUsuario)
        {
            var query = _context.Excusas
                .Include(e => e.Estudiante)
                .AsQueryable();

            if (!esSuperUsuario)
                query = query.Where(e => e.IdInstitucionEducativa == idInstitucion);

            return await query
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
                // Manejar la excepci�n, por ejemplo, registr�ndola o lanz�ndola nuevamente
                Console.WriteLine($"Error al obtener excusas con evidencia: {ex.Message}");
                throw; // O puedes optar por manejarla de otra manera seg�n tu l�gica de negocio

                //return (IEnumerable<Excusa>)excusa;
            }

         
        }

        public async Task<EvidenciaExcusa?> ObtenerEvidenciaPorExcusaAsync(int idExcusa)
        {
            return await _context.EvidenciasExcusa
                .FirstOrDefaultAsync(ev => ev.IdExcusa == idExcusa);
        }

        public async Task ActualizarDecisionAsync(
            int idExcusa, string estado, string motivoDecision, int idAdministrador)
        {
            var excusa = await _context.Excusas.FindAsync(idExcusa);
            if (excusa == null) return;
            excusa.Estado          = estado;
            excusa.MotivoDecision  = motivoDecision;
            excusa.IdAdministrador = idAdministrador;
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarEstadoAsync(int idExcusa, string estado, string motivoDecision)
        {
            var excusa = await _context.Excusas.FindAsync(idExcusa);
            if (excusa == null) return;
            excusa.Estado         = estado;
            excusa.MotivoDecision = motivoDecision;
            await _context.SaveChangesAsync();
        }

        public async Task<Excusa?> ObtenerConEstudianteYAcudienteAsync(int idExcusa)
        {
            return await _context.Excusas
                .Include(e => e.Estudiante).ThenInclude(est => est!.Acudiente)
                .FirstOrDefaultAsync(e => e.IdExcusa == idExcusa);
        }

        public async Task<(List<Excusa> Excusas, int TotalRegistros)> ObtenerPaginadoAsync(
            int pagina, int registrosPorPagina, int idInstitucion, bool esSuperUsuario)
        {
            var query = _context.Excusas
                .Include(e => e.Estudiante).ThenInclude(est => est!.TipoDocumento)
                .Include(e => e.Evidencia)
                .AsQueryable();

            if (!esSuperUsuario)
                query = query.Where(e => e.IdInstitucionEducativa == idInstitucion);

            query = query.OrderByDescending(e => e.FechaHoraRegistro);

            var total   = await query.CountAsync();
            var excusas = await query
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            return (excusas, total);
        }
    }
}
