using Microsoft.EntityFrameworkCore;
using SIRA.Data;
using SIRA.Models;
using SIRA.Repositories.Interfaces;

namespace SIRA.Repositories.Implementations
{
    public class AdministradorRepository : IAdministradorRepository
    {
        private readonly AppDbContext _context;

        public AdministradorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Administrador?> ObtenerPrimeroAsync()
        {
            return await _context.Administradores.FirstOrDefaultAsync();
        }

        public async Task<Administrador?> ObtenerPorUsuarioAsync(int idUsuario)
        {
            return await _context.Administradores
                .FirstOrDefaultAsync(a => a.IdUsuario == idUsuario);
        }

        public async Task AgregarAsync(Administrador administrador)
        {
            await _context.Administradores.AddAsync(administrador);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Administrador>> ObtenerTodosAsync()
        {
            return await _context.Administradores
                .Include(a => a.Usuario)
                .OrderBy(a => a.NombreCompleto)
                .ToListAsync();
        }

        public async Task<List<string>> ObtenerCorreosSuperUsuariosAsync()
        {
            return await _context.Administradores
                .Include(a => a.Usuario)
                .Where(a => a.Usuario != null
                         && a.Usuario.EsActivo
                         && (a.Usuario.EsSuperUsuario || a.Usuario.EsRoot)
                         && a.Correo != null
                         && a.Correo != "")
                .Select(a => a.Correo!)
                .Distinct()
                .ToListAsync();
        }
    }
}
