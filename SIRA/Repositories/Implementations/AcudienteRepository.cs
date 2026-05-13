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

        public async Task<IEnumerable<Acudiente>> ObtenerTodosAsync()
        {
            return await _context.Acudientes
                .Include(a => a.TipoDocumento)
                .OrderBy(a => a.NombreCompleto)
                .ToListAsync();
        }
    }
}
