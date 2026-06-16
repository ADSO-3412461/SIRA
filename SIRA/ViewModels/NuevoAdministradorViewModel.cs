using System.ComponentModel.DataAnnotations;

namespace SIRA.ViewModels
{
    public class NuevoAdministradorViewModel
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El alias de usuario es obligatorio.")]
        public string Alias { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Clave { get; set; } = string.Empty;
    }
}
