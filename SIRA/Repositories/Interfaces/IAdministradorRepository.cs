using SIRA.Models;

namespace SIRA.Repositories.Interfaces
{
    public interface IAdministradorRepository
    {
        Task<Administrador?> ObtenerPrimeroAsync();
        Task<Administrador?> ObtenerPorUsuarioAsync(int idUsuario);
    }
}
