using SIRA.Models.Entities;

namespace SIRA.ViewModels
{
    public class ConsolaSqlViewModel
    {
        public string?            SqlQuery   { get; set; }
        public List<string>       Columnas   { get; set; } = new();
        public List<List<string>> Filas      { get; set; } = new();
        public string?            Mensaje    { get; set; }
        public bool               TieneError { get; set; }
        public IEnumerable<Auditoria> Auditorias { get; set; } = new List<Auditoria>();
    }
}
