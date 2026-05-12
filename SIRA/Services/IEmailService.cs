using SIRA.Models;

namespace SIRA.Services
{
    public interface IEmailService
    {
        Task EnviarNotificacionExcusaAsync(
            Estudiante estudiante,
            Excusa     excusa,
            byte[]     archivoBytes,
            string     archivoNombre,
            string     archivoMime,
            string     toEmail);

        Task EnviarDecisionExcusaAsync(Excusa excusa, string toEmail);
    }
}
