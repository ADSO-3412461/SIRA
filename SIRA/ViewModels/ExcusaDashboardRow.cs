namespace SIRA.ViewModels
{
    public class ExcusaDashboardRow
    {
        public int      IdExcusa           { get; set; }
        public string   NombreEstudiante   { get; set; } = string.Empty;
        public string   TipoDocumento      { get; set; } = string.Empty;
        public string   NumeroDocumento    { get; set; } = string.Empty;
        public string   MotivoInasistencia { get; set; } = string.Empty;
        public string?   Estado             { get; set; } = string.Empty;
        public DateTime FechaRegistro      { get; set; }
        public bool     TieneEvidencia     { get; set; }
    }
}
