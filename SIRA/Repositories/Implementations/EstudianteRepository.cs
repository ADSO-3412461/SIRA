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
                .OrderBy(e => e.NombreCompleto)
                .ToListAsync();
        }

        public async Task<Estudiante?> ObtenerPorIdAsync(int id)
        {
            return await _context.Estudiantes
                .Include(e => e.TipoDocumento)
                .FirstOrDefaultAsync(e => e.IdEstudiante == id);
        }

        public async Task AgregarAsync(Estudiante estudiante)
        {
            await _context.Estudiantes.AddAsync(estudiante);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteDocumentoAsync(string numeroDocumento)
        {
            return await _context.Estudiantes
                .AnyAsync(e => e.NumeroDocumento == numeroDocumento);
        }
    }
}
