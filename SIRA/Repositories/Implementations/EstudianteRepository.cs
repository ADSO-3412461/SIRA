using Microsoft.EntityFrameworkCore;
using SIRA.Data;
using SIRA.Models;
using SIRA.Repositories.Interfaces;

namespace SIRA.Repositories.Implementations
{
    public class EstudianteRepository : IEstudianteRepository
    {
        private readonly AppDbContext _context;

        public EstudianteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Estudiante>> ObtenerTodosAsync()
        {
            return await _context.Estudiantes
                .Include(e => e.TipoDocumento)
                .Include(e => e.Acudiente)
                .Include(e => e.InstitucionEducativa)
                .OrderBy(e => e.NombreCompleto)
                .ToListAsync();
        }

        public async Task<IEnumerable<Estudiante>> ObtenerPorInstitucionAsync(int idInstitucion)
        {
            return await _context.Estudiantes
                .Include(e => e.TipoDocumento)
                .Where(e => e.IdInstitucionEducativa == idInstitucion)
                .OrderBy(e => e.NombreCompleto)
                .ToListAsync();
        }

        public async Task<Estudiante?> ObtenerPorIdAsync(int id)
        {
            return await _context.Estudiantes
                .Include(e => e.TipoDocumento)
                .Include(e => e.Acudiente)
                .Include(e => e.InstitucionEducativa)
                .FirstOrDefaultAsync(e => e.IdEstudiante == id);
        }

        public async Task AgregarAsync(Estudiante estudiante)
        {
            await _context.Estudiantes.AddAsync(estudiante);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Estudiante estudiante)
        {
            var existente = await _context.Estudiantes.FindAsync(estudiante.IdEstudiante);
            if (existente == null) return;

            existente.NombreCompleto         = estudiante.NombreCompleto;
            existente.NumeroDocumento        = estudiante.NumeroDocumento;
            existente.IdTipoDocumento        = estudiante.IdTipoDocumento;
            existente.IdAcudiente            = estudiante.IdAcudiente;
            existente.IdInstitucionEducativa = estudiante.IdInstitucionEducativa;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteDocumentoAsync(string numeroDocumento)
        {
            return await _context.Estudiantes
                .AnyAsync(e => e.NumeroDocumento == numeroDocumento);
        }
    }
}
