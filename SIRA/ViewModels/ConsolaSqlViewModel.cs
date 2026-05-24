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

        // Paginación
        public int  PaginaActual        { get; set; } = 1;
        public int  TotalPaginas        { get; set; }
        public int  TotalRegistros      { get; set; }
        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
    }
}
