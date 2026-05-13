using SIRA.Models;

namespace SIRA.Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObtenerPorAliasAsync(string alias);
        Task<Usuario> AgregarAsync(Usuario usuario);
    }
}
