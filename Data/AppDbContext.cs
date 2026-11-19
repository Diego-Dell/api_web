using ApiWeb.Integrante1_Fichas.Models;
using ApiWeb.Integrante2_ClinicaReproduccion.Models;
using ApiWeb.Integrante3_TratamientosPrevencion.Models;
using ApiWeb.Integrante4_CuidadosAplicaciones.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Data;

/// <summary>
/// DbContext principal de la aplicación conectado a Supabase (PostgreSQL).
/// Contiene los DbSets de los 4 integrantes.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // =========================================================================
    // INTEGRANTE 1 · FICHAS DE GANADO
    // =========================================================================
    public DbSet<FichaGanado> FichasGanado => Set<FichaGanado>();

    // =========================================================================
    // INTEGRANTE 2 · CLÍNICA Y REPRODUCCIÓN
    // =========================================================================
    public DbSet<AtencionClinica> AtencionesClinicas => Set<AtencionClinica>();
    public DbSet<EventoReproduccion> EventosReproduccion => Set<EventoReproduccion>();

    // =========================================================================
    // INTEGRANTE 3 · TRATAMIENTOS Y PREVENCIÓN
    // =========================================================================
    public DbSet<Tratamiento> Tratamientos => Set<Tratamiento>();
    public DbSet<PlanPrevencion> PlanesPrevencion => Set<PlanPrevencion>();

    // =========================================================================
    // INTEGRANTE 4 · CUIDADOS Y APLICACIONES
    // =========================================================================
    public DbSet<ObservacionCuidado> ObservacionesCuidados => Set<ObservacionCuidado>();
    public DbSet<AplicacionIndicacion> AplicacionesIndicaciones => Set<AplicacionIndicacion>();
    public DbSet<CorreccionHistorial> CorreccionesHistorial => Set<CorreccionHistorial>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Integrante 1: fichas_bovinas, con identificación única por granja.
        modelBuilder.ApplyConfiguration(new FichaGanadoConfiguracion());
    }
}
