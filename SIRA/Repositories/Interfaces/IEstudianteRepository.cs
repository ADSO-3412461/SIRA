using SIRA.Models;

namespace SIRA.Repositories.Interfaces
{
    public interface IEstudianteRepository
    {
        Task<IEnumerable<Estudiante>> ObtenerTodosAsync();
        Task<Estudiante?> ObtenerPorIdAsync(int id);
    }
}
