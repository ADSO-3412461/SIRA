using SIRA.Models;

namespace SIRA.Repositories.Interfaces
{
    public interface ITipoDocumentoRepository
    {
        Task<IEnumerable<TipoDocumento>> ObtenerTodosAsync();
    }
}
