using SIRA.Models;

namespace SIRA.Repositories.Interfaces
{
    public interface IEstudianteRepository
    {
        Task<IEnumerable<Estudiante>> ObtenerTodosAsync(int idInstitucion, bool esSuperUsuario);
        Task<IEnumerable<Estudiante>> ObtenerPorInstitucionAsync(int idInstitucion);
        Task<Estudiante?> ObtenerPorIdAsync(int id);
        Task AgregarAsync(Estudiante estudiante);
        Task ActualizarAsync(Estudiante estudiante);
        Task<bool> ExisteDocumentoAsync(string numeroDocumento);
    }
}
