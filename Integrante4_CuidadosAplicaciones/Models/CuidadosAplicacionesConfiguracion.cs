using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiWeb.Integrante4_CuidadosAplicaciones.Models;

public sealed class CuidadosAplicacionesConfiguracion :
    IEntityTypeConfiguration<AplicacionIndicacion>,
    IEntityTypeConfiguration<ObservacionCuidado>,
    IEntityTypeConfiguration<CorreccionHistorial>
{
    public void Configure(EntityTypeBuilder<AplicacionIndicacion> e)
    {
        e.ToTable("aplicaciones", "public", t => t.ExcludeFromMigrations());
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        e.Property(x => x.FichaId).HasColumnName("ficha_id");
        e.Property(x => x.AplicadoPor).HasColumnName("aplicado_por");
        e.Property(x => x.TratamientoId).HasColumnName("tratamiento_id");
        e.Property(x => x.PrevencionId).HasColumnName("prevencion_id");
        e.Property(x => x.FechaHoraReal).HasColumnName("fecha_hora_real");
        e.Property(x => x.Dosis).HasColumnName("dosis");
        e.Property(x => x.Unidad).HasColumnName("unidad");
        e.Property(x => x.Observacion).HasColumnName("observacion");
        e.Property(x => x.Anulado).HasColumnName("anulado");
    }

    public void Configure(EntityTypeBuilder<ObservacionCuidado> e)
    {
        e.ToTable("cuidados", "public");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        e.Property(x => x.FichaId).HasColumnName("ficha_id");
        e.Property(x => x.RegistradoPor).HasColumnName("registrado_por");
        e.Property(x => x.FechaHora).HasColumnName("fecha_hora");
        e.Property(x => x.Observacion).HasColumnName("observacion").IsRequired();
        e.Property(x => x.AccionRealizada).HasColumnName("accion_realizada").IsRequired();
        e.Property(x => x.Anulado).HasColumnName("anulado");
    }

    public void Configure(EntityTypeBuilder<CorreccionHistorial> e)
    {
        e.ToTable("correcciones_registros", "public");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        e.Property(x => x.Tabla).HasColumnName("tabla").IsRequired();
        e.Property(x => x.RegistroId).HasColumnName("registro_id");
        e.Property(x => x.Campo).HasColumnName("campo").IsRequired();
        e.Property(x => x.ValorAnterior).HasColumnName("valor_anterior");
        e.Property(x => x.ValorNuevo).HasColumnName("valor_nuevo");
        e.Property(x => x.Motivo).HasColumnName("motivo").IsRequired();
        e.Property(x => x.CorregidoPor).HasColumnName("corregido_por");
        e.Property(x => x.FechaHora).HasColumnName("fecha_hora");
    }
}
