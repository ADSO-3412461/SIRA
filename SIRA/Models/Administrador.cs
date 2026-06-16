using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIRA.Models
{
    [Table("administrador")]
    public class Administrador
    {
        [Key]
        [Column("id_administrador")]
        public int IdAdministrador { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("correo")]
        public string? Correo { get; set; }

        [Column("nombre_completo")]
        public string? NombreCompleto { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public Usuario? Usuario { get; set; }
    }
}
