using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SIRA.ViewModels
{
    public class AcudienteEditarViewModel
    {
        public int IdAcudiente { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de documento es obligatorio.")]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar el tipo de documento.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar el tipo de documento.")]
        public int IdTipoDocumento { get; set; }

        public List<SelectListItem> TiposDocumento { get; set; } = new();
    }
}
