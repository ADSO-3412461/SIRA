using SIRA.Models;

namespace SIRA.Repositories.Interfaces
{
    public interface IExcusaRepository
    {
        Task AgregarAsync(Excusa excusa);
        Task AgregarEvidenciaAsync(EvidenciaExcusa evidencia);
        Task<Excusa?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Excusa>> ObtenerTodosAsync();
        Task<IEnumerable<Excusa>> ObtenerTodosConEvidenciaAsync();
        Task<EvidenciaExcusa?> ObtenerEvidenciaPorExcusaAsync(int idExcusa);
    }
}
