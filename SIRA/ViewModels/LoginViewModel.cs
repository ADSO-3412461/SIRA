using System.ComponentModel.DataAnnotations;

namespace SIRA.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El alias es obligatorio.")]
        [Display(Name = "Alias")]
        public string Alias { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clave es obligatoria.")]
        [Display(Name = "Clave")]
        [DataType(DataType.Password)]
        public string Clave { get; set; } = string.Empty;
    }
}
