using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiWeb.Integrante1_Fichas.Models;

/// <summary>
/// 1. Mapea FichaGanado a la tabla existente fichas_bovinas, sin crear una tabla nueva.
/// </summary>
public class FichaGanadoConfiguracion : IEntityTypeConfiguration<FichaGanado>
{
    public void Configure(EntityTypeBuilder<FichaGanado> entidad)
    {
        // 1.1. La tabla ya existe en Supabase: las migraciones de EF no deben crearla ni cambiarla.
        entidad.ToTable("fichas_bovinas", "public", tabla => tabla.ExcludeFromMigrations());
        entidad.HasKey(f => f.Id);

        entidad.Property(f => f.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        entidad.Property(f => f.GranjaId).HasColumnName("granja_id");
        entidad.Property(f => f.Identificacion).HasColumnName("identificacion");
        entidad.Property(f => f.Nombre).HasColumnName("nombre");
        entidad.Property(f => f.Raza).HasColumnName("raza");
        entidad.Property(f => f.Etapa).HasColumnName("etapa");
        entidad.Property(f => f.Nacimiento).HasColumnName("nacimiento");
        entidad.Property(f => f.NacimientoEstimado).HasColumnName("nacimiento_estimado");
        entidad.Property(f => f.FechaBaja).HasColumnName("fecha_baja");
        entidad.Property(f => f.MotivoBaja).HasColumnName("motivo_baja");
        entidad.Ignore(f => f.DadoDeBaja);

        // 1.2. La base ya tiene esta restricción; aquí solo se describe para EF.
        entidad.HasIndex(f => new { f.GranjaId, f.Identificacion }).IsUnique();
    }
}
