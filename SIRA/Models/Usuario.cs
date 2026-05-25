using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIRA.Models
{
    [Table("usuario")]
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("alias")]
        [Required]
        public string Alias { get; set; } = string.Empty;

        [Column("clave")]
        [Required]
        public string Clave { get; set; } = string.Empty;

        [Column("es_super_usuario")]
        public bool EsSuperUsuario { get; set; } = false;

        [Column("es_activo")]
        public bool EsActivo { get; set; } = true;

        [Column("es_root")]
        public bool EsRoot { get; set; } = false;
    }
}
