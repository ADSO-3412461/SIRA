using SIRA.Models;

namespace SIRA.Repositories.Interfaces
{
    public interface IEstudianteRepository
    {
        Task<IEnumerable<Estudiante>> ObtenerTodosAsync();
        Task<IEnumerable<Estudiante>> ObtenerPorInstitucionAsync(int idInstitucion);
        Task<Estudiante?> ObtenerPorIdAsync(int id);
        Task AgregarAsync(Estudiante estudiante);
        Task<bool> ExisteDocumentoAsync(string numeroDocumento);
    }
}
