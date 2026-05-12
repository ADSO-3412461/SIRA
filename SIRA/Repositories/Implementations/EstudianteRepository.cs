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
                .OrderBy(e => e.NombreCompleto)
                .ToListAsync();
        }

        public async Task<Estudiante?> ObtenerPorIdAsync(int id)
        {
            return await _context.Estudiantes
                .Include(e => e.TipoDocumento)
                .FirstOrDefaultAsync(e => e.IdEstudiante == id);
        }
    }
}
