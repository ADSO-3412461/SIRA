using Microsoft.EntityFrameworkCore;
using SIRA.Models;
using SIRA.Models.Entities;

namespace SIRA.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Excusa> Excusas => Set<Excusa>();
        public DbSet<Estudiante> Estudiantes => Set<Estudiante>();
        public DbSet<EvidenciaExcusa> EvidenciasExcusa => Set<EvidenciaExcusa>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<TipoDocumento> TiposDocumento => Set<TipoDocumento>();
        public DbSet<Administrador> Administradores => Set<Administrador>();
        public DbSet<Acudiente>    Acudientes       => Set<Acudiente>();
        public DbSet<InstitucionEducativa> InstitucionesEducativas => Set<InstitucionEducativa>();
        public DbSet<Auditoria>            Auditorias             => Set<Auditoria>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InstitucionEducativa>()
                .HasOne(i => i.Administrador)
                .WithMany()
                .HasForeignKey(i => i.IdAdministrador);

            modelBuilder.Entity<Estudiante>()
                .HasOne(e => e.InstitucionEducativa)
                .WithMany()
                .HasForeignKey(e => e.IdInstitucionEducativa);
        }
    }
}
