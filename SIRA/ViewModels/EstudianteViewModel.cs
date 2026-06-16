using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SIRA.ViewModels
{
    public class EstudianteViewModel
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de documento es obligatorio.")]
        public int? IdTipoDocumento { get; set; }

        [Required(ErrorMessage = "El número de documento es obligatorio.")]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe buscar y seleccionar un acudiente.")]
        public int? IdAcudiente { get; set; }

        // Campos auxiliares para el buscador (no se guardan)
        public int? BuscarIdTipoDocumento { get; set; }
        public string? BuscarNumeroDocumento { get; set; }

        public List<SelectListItem> TiposDocumento { get; set; } = new();
    }
}
