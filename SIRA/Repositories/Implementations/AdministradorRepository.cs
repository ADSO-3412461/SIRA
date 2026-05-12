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
    }
}
