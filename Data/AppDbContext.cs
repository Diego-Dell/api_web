using ApiWeb.Integrante1_Fichas.Models;
using ApiWeb.Integrante2_ClinicaReproduccion.Models;
using ApiWeb.Integrante3_TratamientosPrevencion.Models;
using ApiWeb.Integrante4_CuidadosAplicaciones.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Data;

/// <summary>
/// 1. Contexto de datos compartido, conectado a Supabase (PostgreSQL), con las tablas de los 4 integrantes.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // 2. Integrante 1 · Fichas de ganado.
    public DbSet<FichaGanado> FichasGanado => Set<FichaGanado>();

    // 3. Integrante 2 · Clínica y reproducción.
    public DbSet<AtencionClinica> AtencionesClinicas => Set<AtencionClinica>();
    public DbSet<EventoReproduccion> EventosReproduccion => Set<EventoReproduccion>();

    // 4. Integrante 3 · Tratamientos y prevención.
    public DbSet<Tratamiento> Tratamientos => Set<Tratamiento>();
    public DbSet<PlanPrevencion> PlanesPrevencion => Set<PlanPrevencion>();

    // 5. Integrante 4 · Cuidados y aplicaciones.
    public DbSet<ObservacionCuidado> ObservacionesCuidados => Set<ObservacionCuidado>();
    public DbSet<AplicacionIndicacion> AplicacionesIndicaciones => Set<AplicacionIndicacion>();
    public DbSet<CorreccionHistorial> CorreccionesHistorial => Set<CorreccionHistorial>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 6. Integrante 1: mapea fichas_bovinas, con identificación única por granja.
        modelBuilder.ApplyConfiguration(new FichaGanadoConfiguracion());
        var modulo4 = new CuidadosAplicacionesConfiguracion();
        modelBuilder.ApplyConfiguration<AplicacionIndicacion>(modulo4);
        modelBuilder.ApplyConfiguration<ObservacionCuidado>(modulo4);
        modelBuilder.ApplyConfiguration<CorreccionHistorial>(modulo4);
    }
}
