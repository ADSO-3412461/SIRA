using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIRA.Models
{
    [Table("tipo_documento")]
    public class TipoDocumento
    {
        [Key]
        [Column("id_tipo_documento")]
        public int IdTipoDocumento { get; set; }

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("sigla")]
        public string? Sigla { get; set; }

    }
}
