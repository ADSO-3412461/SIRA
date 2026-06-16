using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIRA.Models
{
    [Table("evidencia_excusa")]
    public class EvidenciaExcusa
    {
        [Key]
        [Column("id_evidencia_excusa")]
        public int IdEvidenciaExcusa { get; set; }

        [Column("archivo")]
        public byte[]? Archivo { get; set; }

        [Column("id_excusa")]
        [Required]
        public int IdExcusa { get; set; }

        [ForeignKey(nameof(IdExcusa))]
        public Excusa? Excusa { get; set; }
    }
}
