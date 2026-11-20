using ApiWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Cuenta> Cuentas => Set<Cuenta>();
    public DbSet<Granja> Granjas => Set<Granja>();
    public DbSet<Vinculacion> Vinculaciones => Set<Vinculacion>();
    public DbSet<Invitacion> Invitaciones => Set<Invitacion>();
    public DbSet<FichaBovina> FichasBovinas => Set<FichaBovina>();
    public DbSet<AtencionClinica> AtencionesClinicas => Set<AtencionClinica>();
    public DbSet<EventoReproductivo> EventosReproductivos => Set<EventoReproductivo>();
    public DbSet<Tratamiento> Tratamientos => Set<Tratamiento>();
    public DbSet<Prevencion> Prevenciones => Set<Prevencion>();
    public DbSet<Aplicacion> Aplicaciones => Set<Aplicacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cuenta>().HasIndex(x => x.Identificador).IsUnique();
        modelBuilder.Entity<Granja>().Property(x => x.Hectareas).HasPrecision(12, 2);
        modelBuilder.Entity<Granja>().Property(x => x.Trabajadores).IsRequired();
        modelBuilder.Entity<Granja>().Property(x => x.RUNSA).HasColumnName("RUNSA");
        modelBuilder.Entity<FichaBovina>().HasIndex(x => new { x.GranjaId, x.Identificacion }).IsUnique();
        modelBuilder.Entity<Vinculacion>()
            .HasIndex(x => new { x.CuentaId, x.GranjaId })
            .IsUnique()
            .HasFilter("[Activa] = 1");

        modelBuilder.Entity<FichaBovina>().Property(x => x.Etapa).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<AtencionClinica>().Property(x => x.Tipo).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<EventoReproductivo>().Property(x => x.Tipo).HasConversion<string>().HasMaxLength(40);
        modelBuilder.Entity<EventoReproductivo>().Property(x => x.Aptitud).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<EventoReproductivo>().Property(x => x.Prenez).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<Prevencion>().Property(x => x.Tipo).HasConversion<string>().HasMaxLength(40);
        modelBuilder.Entity<Prevencion>().Property(x => x.Estado).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<Tratamiento>().Property(x => x.Dosis).HasPrecision(12, 3);
        modelBuilder.Entity<Aplicacion>().Property(x => x.Dosis).HasPrecision(12, 3);

        // Restrict deletes to retain the clinical history; records are deactivated or annulled.
        modelBuilder.Entity<Vinculacion>().HasOne(x => x.Cuenta).WithMany(x => x.Vinculaciones)
            .HasForeignKey(x => x.CuentaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Vinculacion>().HasOne(x => x.Granja).WithMany(x => x.Vinculaciones)
            .HasForeignKey(x => x.GranjaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Invitacion>().HasOne(x => x.Granja).WithMany(x => x.Invitaciones)
            .HasForeignKey(x => x.GranjaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Invitacion>().HasOne(x => x.EmitidaPor).WithMany(x => x.InvitacionesEmitidas)
            .HasForeignKey(x => x.EmitidaPorCuentaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<FichaBovina>().HasOne(x => x.Granja).WithMany(x => x.FichasBovinas)
            .HasForeignKey(x => x.GranjaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<AtencionClinica>().HasOne(x => x.FichaBovina).WithMany(x => x.Atenciones)
            .HasForeignKey(x => x.FichaBovinaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<AtencionClinica>().HasOne(x => x.CuentaVeterinario).WithMany()
            .HasForeignKey(x => x.CuentaVeterinarioId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<EventoReproductivo>().HasOne(x => x.FichaBovina).WithMany(x => x.EventosReproductivos)
            .HasForeignKey(x => x.FichaBovinaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<EventoReproductivo>().HasOne(x => x.CuentaVeterinario).WithMany()
            .HasForeignKey(x => x.CuentaVeterinarioId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Tratamiento>().HasOne(x => x.FichaBovina).WithMany()
            .HasForeignKey(x => x.FichaBovinaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Tratamiento>().HasOne(x => x.AtencionClinica).WithMany(x => x.Tratamientos)
            .HasForeignKey(x => x.AtencionClinicaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Tratamiento>().HasOne(x => x.CuentaVeterinario).WithMany()
            .HasForeignKey(x => x.CuentaVeterinarioId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Prevencion>().HasOne(x => x.FichaBovina).WithMany(x => x.Prevenciones)
            .HasForeignKey(x => x.FichaBovinaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Prevencion>().HasOne(x => x.CuentaVeterinario).WithMany()
            .HasForeignKey(x => x.CuentaVeterinarioId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Aplicacion>().HasOne(x => x.FichaBovina).WithMany(x => x.Aplicaciones)
            .HasForeignKey(x => x.FichaBovinaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Aplicacion>().HasOne(x => x.CuentaTrabajador).WithMany()
            .HasForeignKey(x => x.CuentaTrabajadorId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Aplicacion>().HasOne(x => x.Tratamiento).WithMany(x => x.Aplicaciones)
            .HasForeignKey(x => x.TratamientoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Aplicacion>().HasOne(x => x.Prevencion).WithMany(x => x.Aplicaciones)
            .HasForeignKey(x => x.PrevencionId).OnDelete(DeleteBehavior.Restrict);
    }
}
