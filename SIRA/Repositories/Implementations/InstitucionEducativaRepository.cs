using Microsoft.EntityFrameworkCore;
using SIRA.Data;
using SIRA.Models;
using SIRA.Repositories.Interfaces;

namespace SIRA.Repositories.Implementations
{
    public class InstitucionEducativaRepository : IInstitucionEducativaRepository
    {
        private readonly AppDbContext _context;

        public InstitucionEducativaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InstitucionEducativa>> ObtenerTodosAsync()
            => await _context.InstitucionesEducativas
                .Include(i => i.Administrador)
                .OrderBy(i => i.NombreInstitucion)
                .ToListAsync();

        public async Task<InstitucionEducativa?> ObtenerPorIdAsync(int id)
            => await _context.InstitucionesEducativas
                .Include(i => i.Administrador)
                .FirstOrDefaultAsync(i => i.IdInstitucionEducativa == id);

        public async Task AgregarAsync(InstitucionEducativa institucion)
        {
            await _context.InstitucionesEducativas.AddAsync(institucion);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(InstitucionEducativa institucion)
        {
            var existente = await _context.InstitucionesEducativas
                .FindAsync(institucion.IdInstitucionEducativa);
            if (existente == null) return;

            existente.NombreInstitucion = institucion.NombreInstitucion;
            existente.Direccion         = institucion.Direccion;
            existente.Telefono          = institucion.Telefono;
            existente.IdAdministrador   = institucion.IdAdministrador;
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarEstadoAsync(int idInstitucion, bool esActivo)
        {
            var existente = await _context.InstitucionesEducativas.FindAsync(idInstitucion);
            if (existente == null) return;
            existente.EsActivo = esActivo;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteAdministradorAsync(int idAdministrador)
            => await _context.InstitucionesEducativas
                .AnyAsync(i => i.IdAdministrador == idAdministrador);

        public async Task<IEnumerable<Administrador>> ObtenerAdministradoresDisponiblesAsync()
            => await _context.Administradores
                .Include(a => a.Usuario)
                .Where(a => !_context.InstitucionesEducativas
                    .Any(i => i.IdAdministrador == a.IdAdministrador))
                .OrderBy(a => a.NombreCompleto)
                .ToListAsync();
    }
}
