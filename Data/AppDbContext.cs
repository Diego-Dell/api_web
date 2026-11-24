using ApiWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ApiWeb.Data;

// 1. Contexto sobre las tablas que ya existen en Supabase (el esquema lo maneja schema.sql, no las migraciones).
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
    public DbSet<ObservacionCuidado> ObservacionesCuidados => Set<ObservacionCuidado>();
    public DbSet<CorreccionCuidado> CorreccionesCuidados => Set<CorreccionCuidado>();

    // 2. Los roles de la API (Propietario, Veterinario, Trabajador) se guardan como dueno, veterinario y trabajador.
    private static readonly ValueConverter<string, RolGranja> ConvertirRolGranja = new(
        v => v == "Propietario" ? RolGranja.Dueno : Enum.Parse<RolGranja>(v, true),
        v => v == RolGranja.Dueno ? "Propietario" : v.ToString());

    private static readonly ValueConverter<string, RolInvitado> ConvertirRolInvitado = new(
        v => Enum.Parse<RolInvitado>(v, true),
        v => v.ToString());

    protected override void OnModelCreating(ModelBuilder m)
    {
        base.OnModelCreating(m);

        // 3. Tablas de Supabase.
        m.Entity<Cuenta>().ToTable("cuentas");
        m.Entity<Granja>().ToTable("granjas");
        m.Entity<Vinculacion>().ToTable("vinculaciones");
        m.Entity<Invitacion>().ToTable("invitaciones");
        m.Entity<FichaBovina>().ToTable("fichas_bovinas");
        m.Entity<AtencionClinica>().ToTable("atenciones_clinicas");
        m.Entity<EventoReproductivo>().ToTable("eventos_reproductivos");
        m.Entity<Tratamiento>().ToTable("tratamientos");
        m.Entity<Prevencion>().ToTable("prevenciones");
        m.Entity<Aplicacion>().ToTable("aplicaciones");
        m.Entity<ObservacionCuidado>().ToTable("observaciones_cuidados");
        m.Entity<CorreccionCuidado>().ToTable("correcciones_cuidados");
        m.Entity<ObservacionCuidado>().Property(x => x.FichaBovinaId).HasColumnName("ficha_id");
        m.Entity<CorreccionCuidado>().Property(x => x.FichaBovinaId).HasColumnName("ficha_id");
        m.Entity<CorreccionCuidado>().Property(x => x.DatosAnteriores).HasColumnType("jsonb");

        // 4. La contraseña la guarda Supabase Auth, no esta tabla.
        m.Entity<Cuenta>().Ignore(x => x.ClaveHash);

        // 5. Columnas cuyo nombre no sale de pasar la propiedad a snake_case.
        m.Entity<Granja>().Property(x => x.RUNSA).HasColumnName("runsa");
        m.Entity<Invitacion>().Property(x => x.CorreoDestinatario).HasColumnName("correo");
        m.Entity<Invitacion>().Property(x => x.EmitidaPorCuentaId).HasColumnName("emitida_por");
        m.Entity<AtencionClinica>().Property(x => x.FichaBovinaId).HasColumnName("ficha_id");
        m.Entity<AtencionClinica>().Property(x => x.CuentaVeterinarioId).HasColumnName("registrado_por");
        m.Entity<EventoReproductivo>().Property(x => x.FichaBovinaId).HasColumnName("ficha_id");
        m.Entity<EventoReproductivo>().Property(x => x.CuentaVeterinarioId).HasColumnName("registrado_por");
        m.Entity<Tratamiento>().Property(x => x.FichaBovinaId).HasColumnName("ficha_id");
        m.Entity<Tratamiento>().Property(x => x.AtencionClinicaId).HasColumnName("atencion_id");
        m.Entity<Tratamiento>().Property(x => x.CuentaVeterinarioId).HasColumnName("prescrito_por");
        m.Entity<Prevencion>().Property(x => x.FichaBovinaId).HasColumnName("ficha_id");
        m.Entity<Prevencion>().Property(x => x.CuentaVeterinarioId).HasColumnName("programado_por");
        m.Entity<Aplicacion>().Property(x => x.FichaBovinaId).HasColumnName("ficha_id");
        m.Entity<Aplicacion>().Property(x => x.CuentaTrabajadorId).HasColumnName("aplicado_por");

        // 6. Roles como tipos enumerados de PostgreSQL.
        m.Entity<Vinculacion>().Property(x => x.Rol).HasConversion(ConvertirRolGranja);
        m.Entity<Invitacion>().Property(x => x.Rol).HasConversion(ConvertirRolInvitado);

        // 7. Precisión igual a la de Supabase.
        m.Entity<Granja>().Property(x => x.Hectareas).HasPrecision(10, 2);
        m.Entity<Tratamiento>().Property(x => x.Dosis).HasPrecision(10, 3);
        m.Entity<Aplicacion>().Property(x => x.Dosis).HasPrecision(10, 3);

        // 8. Relaciones: se conservan los antecedentes, nunca se borra en cascada.
        m.Entity<Vinculacion>().HasOne(x => x.Cuenta).WithMany(x => x.Vinculaciones).HasForeignKey(x => x.CuentaId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<Vinculacion>().HasOne(x => x.Granja).WithMany(x => x.Vinculaciones).HasForeignKey(x => x.GranjaId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<Invitacion>().HasOne(x => x.Granja).WithMany(x => x.Invitaciones).HasForeignKey(x => x.GranjaId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<Invitacion>().HasOne(x => x.EmitidaPor).WithMany(x => x.InvitacionesEmitidas).HasForeignKey(x => x.EmitidaPorCuentaId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<FichaBovina>().HasOne(x => x.Granja).WithMany(x => x.FichasBovinas).HasForeignKey(x => x.GranjaId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<AtencionClinica>().HasOne(x => x.FichaBovina).WithMany(x => x.Atenciones).HasForeignKey(x => x.FichaBovinaId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<AtencionClinica>().HasOne(x => x.CuentaVeterinario).WithMany().HasForeignKey(x => x.CuentaVeterinarioId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<EventoReproductivo>().HasOne(x => x.FichaBovina).WithMany(x => x.EventosReproductivos).HasForeignKey(x => x.FichaBovinaId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<EventoReproductivo>().HasOne(x => x.CuentaVeterinario).WithMany().HasForeignKey(x => x.CuentaVeterinarioId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<Tratamiento>().HasOne(x => x.FichaBovina).WithMany().HasForeignKey(x => x.FichaBovinaId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<Tratamiento>().HasOne(x => x.AtencionClinica).WithMany(x => x.Tratamientos).HasForeignKey(x => x.AtencionClinicaId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<Tratamiento>().HasOne(x => x.CuentaVeterinario).WithMany().HasForeignKey(x => x.CuentaVeterinarioId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<Prevencion>().HasOne(x => x.FichaBovina).WithMany(x => x.Prevenciones).HasForeignKey(x => x.FichaBovinaId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<Prevencion>().HasOne(x => x.CuentaVeterinario).WithMany().HasForeignKey(x => x.CuentaVeterinarioId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<Aplicacion>().HasOne(x => x.FichaBovina).WithMany(x => x.Aplicaciones).HasForeignKey(x => x.FichaBovinaId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<Aplicacion>().HasOne(x => x.CuentaTrabajador).WithMany().HasForeignKey(x => x.CuentaTrabajadorId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<Aplicacion>().HasOne(x => x.Tratamiento).WithMany(x => x.Aplicaciones).HasForeignKey(x => x.TratamientoId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<Aplicacion>().HasOne(x => x.Prevencion).WithMany(x => x.Aplicaciones).HasForeignKey(x => x.PrevencionId).OnDelete(DeleteBehavior.Restrict);

        // 9. El resto de columnas: PascalCase de C# a snake_case de PostgreSQL.
        foreach (var entidad in m.Model.GetEntityTypes())
            foreach (var propiedad in entidad.GetProperties())
                if (propiedad.GetColumnName() == propiedad.Name)
                    propiedad.SetColumnName(SnakeCase(propiedad.Name));
    }

    private static string SnakeCase(string nombre) => string.Concat(nombre.Select((c, i) =>
        i > 0 && char.IsUpper(c) ? "_" + char.ToLowerInvariant(c) : char.ToLowerInvariant(c).ToString()));
}
