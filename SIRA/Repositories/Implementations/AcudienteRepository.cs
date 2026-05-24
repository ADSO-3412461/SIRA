using Microsoft.EntityFrameworkCore;
using SIRA.Data;
using SIRA.Models;
using SIRA.Repositories.Interfaces;

namespace SIRA.Repositories.Implementations
{
    public class AcudienteRepository : IAcudienteRepository
    {
        private readonly AppDbContext _context;

        public AcudienteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AgregarAsync(Acudiente acudiente)
        {
            await _context.Acudientes.AddAsync(acudiente);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteDocumentoAsync(string numeroDocumento)
        {
            return await _context.Acudientes
                .AnyAsync(a => a.NumeroDocumento == numeroDocumento);
        }

        public async Task<bool> ExisteCorreoAsync(string correo)
        {
            return await _context.Acudientes
                .AnyAsync(a => a.Correo == correo);
        }

        public async Task<Acudiente?> BuscarPorDocumentoAsync(int idTipoDocumento, string numeroDocumento)
        {
            return await _context.Acudientes
                .FirstOrDefaultAsync(a =>
                    a.IdTipoDocumento == idTipoDocumento &&
                    a.NumeroDocumento == numeroDocumento);
        }

        public async Task<IEnumerable<Acudiente>> ObtenerTodosAsync(int idInstitucion, bool esSuperUsuario)
        {
            var query = _context.Acudientes
                .Include(a => a.TipoDocumento)
                .AsQueryable();

            if (!esSuperUsuario)
                query = query.Where(a => a.IdInstitucionEducativa == idInstitucion);

            return await query
                .OrderBy(a => a.NombreCompleto)
                .ToListAsync();
        }

        public async Task<IEnumerable<Acudiente>> ObtenerTodosConInstitucionAsync(int idInstitucion, bool esSuperUsuario)
        {
            var query = _context.Acudientes
                .Include(a => a.TipoDocumento)
                .Include(a => a.InstitucionEducativa)
                .AsQueryable();

            if (!esSuperUsuario)
                query = query.Where(a => a.IdInstitucionEducativa == idInstitucion);

            return await query
                .OrderBy(a => a.NombreCompleto)
                .ToListAsync();
        }

        public async Task<Acudiente?> ObtenerPorIdAsync(int idAcudiente)
        {
            return await _context.Acudientes
                .Include(a => a.TipoDocumento)
                .FirstOrDefaultAsync(a => a.IdAcudiente == idAcudiente);
        }

        public async Task ActualizarAsync(Acudiente acudiente)
        {
            var existente = await _context.Acudientes.FindAsync(acudiente.IdAcudiente);
            if (existente == null) return;

            existente.NombreCompleto         = acudiente.NombreCompleto;
            existente.IdTipoDocumento        = acudiente.IdTipoDocumento;
            existente.NumeroDocumento        = acudiente.NumeroDocumento;
            existente.Correo                 = acudiente.Correo;
            existente.IdInstitucionEducativa = acudiente.IdInstitucionEducativa;

            await _context.SaveChangesAsync();
        }
    }
}
