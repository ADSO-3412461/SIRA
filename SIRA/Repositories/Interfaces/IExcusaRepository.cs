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
        Task ActualizarDecisionAsync(int idExcusa, string estado, string motivoDecision, int idAdministrador);
        Task ActualizarEstadoAsync(int idExcusa, string estado, string motivoDecision);
        Task<Excusa?> ObtenerConEstudianteYAcudienteAsync(int idExcusa);
        Task<(List<Excusa> Excusas, int TotalRegistros)> ObtenerPaginadoAsync(int pagina, int registrosPorPagina);
    }
}
