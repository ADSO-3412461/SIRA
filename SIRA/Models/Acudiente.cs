using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIRA.Models.Entities;

namespace SIRA.Models
{
    [Table("acudiente")]
    public class Acudiente
    {
        [Key]
        [Column("id_acudiente")]
        public int IdAcudiente { get; set; }

        [Column("nombre_completo")]
        public string? NombreCompleto { get; set; }

        [Column("numero_documento")]
        public string? NumeroDocumento { get; set; }

        [Column("correo")]
        public string? Correo { get; set; }

        [Column("contrasena")]
        public string? Contrasena { get; set; }

        [Column("id_tipo_documento")]
        public int IdTipoDocumento { get; set; }

        [ForeignKey(nameof(IdTipoDocumento))]
        public TipoDocumento? TipoDocumento { get; set; }

        [Column("id_institucion_educativa")]
        public int IdInstitucionEducativa { get; set; }

        [ForeignKey(nameof(IdInstitucionEducativa))]
        public InstitucionEducativa? InstitucionEducativa { get; set; }
    }
}
