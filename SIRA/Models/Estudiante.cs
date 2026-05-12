using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIRA.Models
{
    [Table("estudiante")]
    public class Estudiante
    {
        [Key]
        [Column("id_estudiante")]
        public int IdEstudiante { get; set; }

        [Column("nombre_completo")]
        public string? NombreCompleto { get; set; }

        [Column("numero_documento")]
        public string? NumeroDocumento { get; set; }

        [Column("id_tipo_documento")]
        [Required]
        public int IdTipoDocumento { get; set; }

        [ForeignKey(nameof(IdTipoDocumento))]
        public TipoDocumento? TipoDocumento { get; set; }

        [Column("id_acudiente")]
        public int? IdAcudiente { get; set; }

        [ForeignKey(nameof(IdAcudiente))]
        public Acudiente? Acudiente { get; set; }

        public ICollection<Excusa> Excusas { get; set; } = new List<Excusa>();
    }
}
