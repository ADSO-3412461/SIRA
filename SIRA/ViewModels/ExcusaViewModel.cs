using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SIRA.ViewModels
{
    public class ExcusaViewModel
    {
        [Required(ErrorMessage = "Seleccione un estudiante.")]
        [Display(Name = "Estudiante")]
        public int? IdEstudiante { get; set; }

        [Required(ErrorMessage = "Ingrese el motivo de la inasistencia.")]
        [StringLength(1000, MinimumLength = 10,
            ErrorMessage = "El motivo debe tener entre 10 y 1000 caracteres.")]
        [Display(Name = "Motivo de la inasistencia")]
        public string MotivoInasistencia { get; set; } = string.Empty;

        [Display(Name = "Documento de evidencia")]
        public IFormFile? Evidencia { get; set; }

        // Poblado por el controller en GET — no se bindea en POST
        public List<SelectListItem> Estudiantes { get; set; } = new();
    }
}
