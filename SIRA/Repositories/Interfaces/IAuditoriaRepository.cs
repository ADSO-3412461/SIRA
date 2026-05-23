using SIRA.Models.Entities;

namespace SIRA.Repositories.Interfaces
{
    public interface IAuditoriaRepository
    {
        Task RegistrarAsync(Auditoria auditoria);
        Task<IEnumerable<Auditoria>> ObtenerUltimasAsync(int cantidad = 50);
    }
}
