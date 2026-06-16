using SIRA.Models;

namespace SIRA.Repositories.Interfaces
{
    public interface IInstitucionEducativaRepository
    {
        Task<IEnumerable<InstitucionEducativa>> ObtenerTodosAsync();
        Task<IEnumerable<InstitucionEducativa>> ObtenerParaDropdownAsync();
        Task<InstitucionEducativa?> ObtenerPorIdAsync(int id);
        Task AgregarAsync(InstitucionEducativa institucion);
        Task ActualizarAsync(InstitucionEducativa institucion);
        Task ActualizarEstadoAsync(int idInstitucion, bool esActivo);
        Task<bool> ExisteAdministradorAsync(int idAdministrador);
        Task<IEnumerable<Administrador>> ObtenerAdministradoresDisponiblesAsync();
    }
}
