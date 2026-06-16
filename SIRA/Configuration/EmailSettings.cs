namespace SIRA.Configuration
{
    public class EmailSettings
    {
        public string SmtpHost     { get; set; } = string.Empty;
        public int    SmtpPort     { get; set; }
        public string SmtpUser     { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string FromEmail    { get; set; } = string.Empty;
        public string FromName     { get; set; } = string.Empty;
        public bool   EnableSsl    { get; set; }
        public string ToEmail      { get; set; } = string.Empty;
    }
}
