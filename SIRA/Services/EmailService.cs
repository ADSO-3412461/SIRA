using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using SIRA.Configuration;
using SIRA.Models;

namespace SIRA.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings        _cfg;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
        {
            _cfg    = options.Value;
            _logger = logger;
        }

        public async Task EnviarNotificacionExcusaAsync(
            Estudiante estudiante,
            Excusa     excusa,
            byte[]     archivoBytes,
            string     archivoNombre,
            string     archivoMime,
            string     toEmail)
        {
            using var mensaje = new MailMessage();
            mensaje.From       = new MailAddress(_cfg.SmtpUser, _cfg.FromName);
            mensaje.To.Add(toEmail);
            //mensaje.To.Add(_cfg.ToEmail);
            mensaje.Subject    = $"Nueva excusa registrada — {estudiante.NombreCompleto ?? "Estudiante"}";
            mensaje.IsBodyHtml = true;
            mensaje.Body       = ConstruirHtml(estudiante, excusa);

            using var adjuntoStream = new MemoryStream(archivoBytes);
            mensaje.Attachments.Add(new Attachment(adjuntoStream, archivoNombre, archivoMime));

            using var smtp = new SmtpClient(_cfg.SmtpHost, _cfg.SmtpPort)
            {
                Credentials = new NetworkCredential(_cfg.SmtpUser, _cfg.SmtpPassword),
                EnableSsl   = _cfg.EnableSsl
            };

            await smtp.SendMailAsync(mensaje);

            _logger.LogInformation(
                "Correo de excusa {IdExcusa} enviado a {ToEmail}.",
                excusa.IdExcusa, toEmail);
        }

        public async Task EnviarDecisionExcusaAsync(Excusa excusa, string toEmail)
        {
            var nombreEstudiante = excusa.Estudiante?.NombreCompleto ?? "Estudiante";
            var estado           = excusa.Estado ?? "—";

            using var mensaje = new MailMessage();
            mensaje.From       = new MailAddress(_cfg.SmtpUser, _cfg.FromName);
            mensaje.To.Add(toEmail);
            mensaje.Subject    = $"Notificacion Cambio Estado Excusa - {nombreEstudiante}";
            mensaje.IsBodyHtml = true;
            mensaje.Body       = ConstruirHtmlDecision(excusa);

            using var smtp = new SmtpClient(_cfg.SmtpHost, _cfg.SmtpPort)
            {
                Credentials = new NetworkCredential(_cfg.SmtpUser, _cfg.SmtpPassword),
                EnableSsl   = _cfg.EnableSsl
            };

            await smtp.SendMailAsync(mensaje);

            _logger.LogInformation(
                "Correo de decisión ({Estado}) para excusa {IdExcusa} enviado a {ToEmail}.",
                estado, excusa.IdExcusa, toEmail);
        }

        // ── HTML del correo ───────────────────────────────────────────────────

        private static string ConstruirHtml(Estudiante est, Excusa exc)
        {
            var fecha  = exc.FechaHoraRegistro?.ToString("dd/MM/yyyy HH:mm") ?? "—";
            var motivo = System.Net.WebUtility.HtmlEncode(exc.MotivoInasistencia ?? "—");
            var estado = exc.Estado ?? "Por revisar";

            return $"""
                <!DOCTYPE html>
                <html lang="es">
                <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
                <body style="margin:0;padding:24px;background:#f4f6f9;font-family:'Segoe UI',Arial,sans-serif;">
                  <div style="max-width:600px;margin:0 auto;background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 10px rgba(0,0,0,.1);">

                    <!-- Cabecera -->
                    <div style="background:#1a2e4a;padding:28px 32px;">
                      <h1 style="margin:0;color:#fff;font-size:20px;font-weight:700;">
                        SIRA — Nueva Excusa Registrada
                      </h1>
                      <p style="margin:6px 0 0;color:rgba(255,255,255,.7);font-size:13px;">
                        Sistema Integral de Registro de Ausencias · SENA CAE Curumaní
                      </p>
                    </div>

                    <!-- Cuerpo -->
                    <div style="padding:28px 32px;">

                      <h2 style="margin:0 0 14px;color:#1a2e4a;font-size:15px;font-weight:700;text-transform:uppercase;letter-spacing:.05em;">
                        Datos del Estudiante
                      </h2>
                      <table style="width:100%;border-collapse:collapse;font-size:14px;margin-bottom:24px;">
                        <tr style="border-bottom:1px solid #e9ecef;">
                          <td style="padding:10px 0;color:#6c757d;width:42%;">Nombre completo</td>
                          <td style="padding:10px 0;color:#212529;font-weight:600;">{est.NombreCompleto ?? "—"}</td>
                        </tr>
                        <tr style="border-bottom:1px solid #e9ecef;">
                          <td style="padding:10px 0;color:#6c757d;">Tipo de documento</td>
                          <td style="padding:10px 0;color:#212529;">{est.TipoDocumento?.Sigla ?? "—"}</td>
                        </tr>
                        <tr>
                          <td style="padding:10px 0;color:#6c757d;">Número de documento</td>
                          <td style="padding:10px 0;color:#212529;">{est.NumeroDocumento ?? "—"}</td>
                        </tr>
                      </table>

                      <h2 style="margin:0 0 14px;color:#1a2e4a;font-size:15px;font-weight:700;text-transform:uppercase;letter-spacing:.05em;">
                        Datos de la Excusa
                      </h2>
                      <table style="width:100%;border-collapse:collapse;font-size:14px;">
                        <tr style="border-bottom:1px solid #e9ecef;">
                          <td style="padding:10px 0;color:#6c757d;width:42%;">Fecha de registro</td>
                          <td style="padding:10px 0;color:#212529;">{fecha}</td>
                        </tr>
                        <tr style="border-bottom:1px solid #e9ecef;">
                          <td style="padding:10px 0;color:#6c757d;">Estado</td>
                          <td style="padding:10px 0;">
                            <span style="display:inline-block;background:#ffc107;color:#000;padding:3px 12px;border-radius:12px;font-size:12px;font-weight:600;">
                              {estado}
                            </span>
                          </td>
                        </tr>
                        <tr>
                          <td style="padding:10px 0;color:#6c757d;vertical-align:top;">Motivo</td>
                          <td style="padding:10px 0;color:#212529;line-height:1.5;">{motivo}</td>
                        </tr>
                      </table>

                      <div style="margin-top:24px;padding:14px 16px;background:#f4f6f9;border-radius:6px;font-size:13px;color:#495057;">
                        <strong>Evidencia adjunta:</strong> el archivo de soporte se encuentra adjunto en este correo.
                      </div>
                    </div>

                    <!-- Pie -->
                    <div style="padding:16px 32px;background:#f4f6f9;border-top:1px solid #e9ecef;">
                      <p style="margin:0;font-size:12px;color:#6c757d;">
                        Mensaje generado automáticamente por SIRA. No responda a este correo.
                      </p>
                    </div>

                  </div>
                </body>
                </html>
                """;
        }
        private static string ConstruirHtmlDecision(Excusa exc)
        {
            var nombreEstudiante = System.Net.WebUtility.HtmlEncode(exc.Estudiante?.NombreCompleto ?? "—");
            var fecha   = exc.FechaHoraRegistro?.ToString("dd/MM/yyyy HH:mm") ?? "—";
            var estado  = exc.Estado ?? "—";
            var motivo  = System.Net.WebUtility.HtmlEncode(exc.MotivoDecision ?? "—");

            var (colorEstado, icono) = estado == "Aprobada"
                ? ("#198754", "✔")
                : ("#dc3545", "✖");

            return $"""
                <!DOCTYPE html>
                <html lang="es">
                <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
                <body style="margin:0;padding:24px;background:#f4f6f9;font-family:'Segoe UI',Arial,sans-serif;">
                  <div style="max-width:600px;margin:0 auto;background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 10px rgba(0,0,0,.1);">

                    <!-- Cabecera -->
                    <div style="background:#1a2e4a;padding:28px 32px;">
                      <h1 style="margin:0;color:#fff;font-size:20px;font-weight:700;">
                        SIRA — Decisión sobre su Excusa
                      </h1>
                      <p style="margin:6px 0 0;color:rgba(255,255,255,.7);font-size:13px;">
                        Sistema Integral de Registro de Ausencias · SENA CAE Curumaní
                      </p>
                    </div>

                    <!-- Cuerpo -->
                    <div style="padding:28px 32px;">

                      <p style="font-size:15px;color:#212529;margin:0 0 20px;">
                        La excusa del estudiante <strong>{nombreEstudiante}</strong>
                        ha sido revisada por el administrador.
                      </p>

                      <!-- Badge de estado -->
                      <div style="text-align:center;margin-bottom:24px;">
                        <span style="display:inline-block;background:{colorEstado};color:#fff;
                                     padding:10px 28px;border-radius:20px;font-size:18px;font-weight:700;">
                          {icono} {estado}
                        </span>
                      </div>

                      <table style="width:100%;border-collapse:collapse;font-size:14px;">
                        <tr style="border-bottom:1px solid #e9ecef;">
                          <td style="padding:10px 0;color:#6c757d;width:42%;">Estudiante</td>
                          <td style="padding:10px 0;color:#212529;font-weight:600;">{nombreEstudiante}</td>
                        </tr>
                        <tr style="border-bottom:1px solid #e9ecef;">
                          <td style="padding:10px 0;color:#6c757d;">Fecha de excusa</td>
                          <td style="padding:10px 0;color:#212529;">{fecha}</td>
                        </tr>
                        <tr>
                          <td style="padding:10px 0;color:#6c757d;vertical-align:top;">Motivo de la decisión</td>
                          <td style="padding:10px 0;color:#212529;line-height:1.5;">{motivo}</td>
                        </tr>
                      </table>

                    </div>

                    <!-- Pie -->
                    <div style="padding:16px 32px;background:#f4f6f9;border-top:1px solid #e9ecef;">
                      <p style="margin:0;font-size:12px;color:#6c757d;">
                        Mensaje generado automáticamente por SIRA. No responda a este correo.
                      </p>
                    </div>

                  </div>
                </body>
                </html>
                """;
        }
    }
}
