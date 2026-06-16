namespace SIRA.ViewModels
{
    public class DashboardViewModel
    {
        public List<ExcusaDashboardRow> Filas          { get; set; } = new();
        public int                      PaginaActual   { get; set; }
        public int                      TotalPaginas   { get; set; }
        public int                      TotalRegistros { get; set; }
        public bool TienePaginaAnterior  => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
    }
}
