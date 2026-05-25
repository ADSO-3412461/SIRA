using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIRA.Models.Entities
{
    [Table("auditoria")]
    public class Auditoria
    {
        [Key]
        [Column("id_auditoria")]
        public int IdAuditoria { get; set; }

        [Column("fecha_hora")]
        public DateTime FechaEjecucion { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("alias_usuario")]
        public string Alias { get; set; } = string.Empty;

        [Column("script_ejecutado")]
        public string SqlEjecutado { get; set; } = string.Empty;

        [Column("resultado")]
        public string? Resultado { get; set; }

        [Column("mensaje_error")]
        public string? MensajeError { get; set; }
    }
}
