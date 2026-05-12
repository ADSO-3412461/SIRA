using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIRA.Models
{
    [Table("excusa")]
    public class Excusa
    {
        [Key]
        [Column("id_excusa")]
        public int IdExcusa { get; set; }

        [Column("fecha_hora_registro")]
        public DateTime? FechaHoraRegistro { get; set; }

        [Column("motivo_inasistencia")]
        public string? MotivoInasistencia { get; set; }

        [Column("estado")]
        public string? Estado { get; set; } = "Pendiente";

        [Column("motivo_decision")]
        public string? MotivoDecision { get; set; }

        [Column("id_estudiante")]
        [Required]
        public int IdEstudiante { get; set; }

        [Column("id_administrador")]
        public int? IdAdministrador { get; set; }

        [ForeignKey(nameof(IdEstudiante))]
        public Estudiante? Estudiante { get; set; }

        [ForeignKey(nameof(IdAdministrador))]
        public Administrador? Administrador { get; set; }

        public EvidenciaExcusa? Evidencia { get; set; }
    }
}
