using Microsoft.EntityFrameworkCore;
using SIRA.Data;
using SIRA.Models;
using SIRA.Repositories.Interfaces;

namespace SIRA.Repositories.Implementations
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> ObtenerPorAliasAsync(string alias)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Alias == alias);
        }

        public async Task<Usuario> AgregarAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task ActualizarEstadoAsync(int idUsuario, bool esActivo)
        {
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null) return;
            usuario.EsActivo = esActivo;
            await _context.SaveChangesAsync();
        }
    }
}
