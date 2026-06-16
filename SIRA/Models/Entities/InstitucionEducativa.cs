using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIRA.Models
{
    [Table("institucion_educativa")]
    public class InstitucionEducativa
    {
        [Key]
        [Column("id_institucion_educativa")]
        public int IdInstitucionEducativa { get; set; }

        [Column("nombre_institucion")]
        public string? NombreInstitucion { get; set; }

        [Column("direccion")]
        public string? Direccion { get; set; }

        [Column("telefono")]
        public string? Telefono { get; set; }

        [Column("id_administrador")]
        public int IdAdministrador { get; set; }

        [Column("es_activo")]
        public bool EsActivo { get; set; } = true;

        [ForeignKey(nameof(IdAdministrador))]
        public Administrador? Administrador { get; set; }
    }
}
