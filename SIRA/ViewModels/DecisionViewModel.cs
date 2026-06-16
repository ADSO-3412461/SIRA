using System.ComponentModel.DataAnnotations;

namespace SIRA.ViewModels
{
    public class DecisionViewModel
    {
        [Required]
        public int IdExcusa { get; set; }

        [Required(ErrorMessage = "Seleccione una decisión.")]
        public string Estado { get; set; } = string.Empty;

        [Required(ErrorMessage = "El motivo de la decisión es obligatorio.")]
        public string MotivoDecision { get; set; } = string.Empty;
    }
}
