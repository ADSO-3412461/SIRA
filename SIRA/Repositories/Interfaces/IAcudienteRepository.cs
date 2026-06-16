using SIRA.Models;

namespace SIRA.Repositories.Interfaces
{
    public interface IAcudienteRepository
    {
        Task AgregarAsync(Acudiente acudiente);
        Task<bool> ExisteDocumentoAsync(string numeroDocumento);
        Task<bool> ExisteCorreoAsync(string correo);
        Task<Acudiente?> BuscarPorDocumentoAsync(int idTipoDocumento, string numeroDocumento);
        Task<IEnumerable<Acudiente>> ObtenerTodosAsync(int idInstitucion, bool esSuperUsuario);
        Task<IEnumerable<Acudiente>> ObtenerTodosConInstitucionAsync(int idInstitucion, bool esSuperUsuario);
        Task<Acudiente?> ObtenerPorIdAsync(int idAcudiente);
        Task ActualizarAsync(Acudiente acudiente);
    }
}
