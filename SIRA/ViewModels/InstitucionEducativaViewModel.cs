using System.ComponentModel.DataAnnotations;

namespace SIRA.ViewModels
{
    public class InstitucionEducativaViewModel
    {
        public int IdInstitucionEducativa { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string NombreInstitucion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un administrador.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un administrador.")]
        public int IdAdministrador { get; set; }

        public bool EsActivo { get; set; } = true;
    }
}
