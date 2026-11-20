
using ApiWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Data;

public static class DbInitializer
{
    // GUID fijos para los nuevos datos de prueba
    private static Guid Id(int numero) =>
        Guid.Parse($"00000000-0000-0000-0000-{numero:000000000000}");

    // Inserta solamente si el registro no existe
    private static async Task AgregarSiFalta<T>(
        AppDbContext db, Guid id, T entidad) where T : class
    {
        bool existe = await db.Set<T>()
            .AnyAsync(x => EF.Property<Guid>(x, "Id") == id);

        if (!existe)
            db.Set<T>().Add(entidad);
    }

    public static async Task InicializarAsync(AppDbContext db)
    {
        // IDENTIFICADORES EXISTENTES
        var granja1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var granja2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var lunaId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
        var moraId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2");
        var estrellaId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3");

        // IDENTIFICADORES NUEVOS
        var duenoId = Id(101);
        var veterinarioId = Id(102);
        var trabajadorId = Id(103);

        var atencion1Id = Id(301);
        var atencion2Id = Id(302);
        var atencion3Id = Id(303);

        var tratamiento1Id = Id(501);
        var tratamiento2Id = Id(502);

        var prevencion1Id = Id(601);
        var prevencion2Id = Id(602);
        var prevencion3Id = Id(603);

        // 1. CUENTAS
        var passwordHasher = new PasswordHasher<Cuenta>();

        Cuenta CrearCuenta(Guid id, string identificador, string nombre)
        {
            var cuenta = new Cuenta
            {
                Id = id,
                Identificador = identificador,
                Nombre = nombre,
                Activa = true,
                AdministradorTecnico = false
            };

            cuenta.ClaveHash = passwordHasher.HashPassword(
                cuenta, "VetLinkDemo#2026");

            return cuenta;
        }

        await AgregarSiFalta(db, duenoId,
            CrearCuenta(duenoId, "dueno.demo", "Propietario Demo"));

        await AgregarSiFalta(db, veterinarioId,
            CrearCuenta(veterinarioId, "veterinario.demo", "Veterinaria Demo"));

        await AgregarSiFalta(db, trabajadorId,
            CrearCuenta(trabajadorId, "trabajador.demo", "Trabajador Demo"));

        // 2. GRANJAS
        await AgregarSiFalta(db, granja1Id, new Granja
        {
            Id = granja1Id,
            Nombre = "Hacienda Demo",
            Hectareas = 150,
            Trabajadores = 5,
            RUNSA = "DEMO-001",
            Confirmada = true
        });

        await AgregarSiFalta(db, granja2Id, new Granja
        {
            Id = granja2Id,
            Nombre = "Granja Los Pinos",
            Hectareas = 80,
            Trabajadores = 3,
            RUNSA = "DEMO-002",
            Confirmada = true
        });

        await db.SaveChangesAsync();

        // 3. VINCULACIONES
        await AgregarSiFalta(db, Id(201), new Vinculacion
        {
            Id = Id(201),
            CuentaId = duenoId,
            GranjaId = granja1Id,
            Rol = "Dueno",
            Activa = true
        });

        await AgregarSiFalta(db, Id(202), new Vinculacion
        {
            Id = Id(202),
            CuentaId = veterinarioId,
            GranjaId = granja1Id,
            Rol = "Veterinario",
            Activa = true
        });

        await AgregarSiFalta(db, Id(203), new Vinculacion
        {
            Id = Id(203),
            CuentaId = trabajadorId,
            GranjaId = granja1Id,
            Rol = "Trabajador",
            Activa = true
        });

        // 4. INVITACIONES
        await AgregarSiFalta(db, Id(211), new Invitacion
        {
            Id = Id(211),
            GranjaId = granja1Id,
            EmitidaPorCuentaId = duenoId,
            Rol = "Veterinario",
            Vigente = false,
            Aceptada = true,
            CorreoDestinatario = "veterinario@ejemplo.com",
            FechaVencimiento = new DateTime(2026, 12, 31)
        });

        await AgregarSiFalta(db, Id(212), new Invitacion
        {
            Id = Id(212),
            GranjaId = granja1Id,
            EmitidaPorCuentaId = duenoId,
            Rol = "Trabajador",
            Vigente = true,
            Aceptada = false,
            CorreoDestinatario = "nuevo@ejemplo.com",
            FechaVencimiento = new DateTime(2026, 12, 31)
        });

        // 5. FICHAS BOVINAS
        await AgregarSiFalta(db, lunaId, new FichaBovina
        {
            Id = lunaId,
            GranjaId = granja1Id,
            Identificacion = "A-001",
            Nombre = "Luna",
            Raza = "Holstein",
            Etapa = Etapa.Vaca,
            Nacimiento = new DateOnly(2022, 5, 10),
            NacimientoEstimado = false
        });

        await AgregarSiFalta(db, moraId, new FichaBovina
        {
            Id = moraId,
            GranjaId = granja1Id,
            Identificacion = "A-002",
            Nombre = "Mora",
            Raza = "Pardo Suizo",
            Etapa = Etapa.Vaquilla,
            Nacimiento = new DateOnly(2025, 2, 15),
            NacimientoEstimado = false
        });

        await AgregarSiFalta(db, estrellaId, new FichaBovina
        {
            Id = estrellaId,
            GranjaId = granja2Id,
            Identificacion = "B-001",
            Nombre = "Estrella",
            Raza = "Holstein",
            Etapa = Etapa.Ternera,
            Nacimiento = new DateOnly(2026, 8, 20),
            NacimientoEstimado = false
        });

        await db.SaveChangesAsync();

        // 6. ATENCIONES CLINICAS
        await AgregarSiFalta(db, atencion1Id, new AtencionClinica
        {
            Id = atencion1Id,
            FichaBovinaId = lunaId,
            CuentaVeterinarioId = veterinarioId,
            Fecha = new DateTime(2026, 11, 10, 9, 0, 0),
            Tipo = TipoAtencion.Consulta,
            Detalle = "Revision general y evaluacion clinica.",
            Anulado = false
        });

        await AgregarSiFalta(db, atencion2Id, new AtencionClinica
        {
            Id = atencion2Id,
            FichaBovinaId = moraId,
            CuentaVeterinarioId = veterinarioId,
            Fecha = new DateTime(2026, 11, 12, 10, 0, 0),
            Tipo = TipoAtencion.Control,
            Detalle = "Control sanitario preventivo.",
            Anulado = false
        });

        await AgregarSiFalta(db, atencion3Id, new AtencionClinica
        {
            Id = atencion3Id,
            FichaBovinaId = lunaId,
            CuentaVeterinarioId = veterinarioId,
            Fecha = new DateTime(2026, 11, 15, 8, 30, 0),
            Tipo = TipoAtencion.Consulta,
            Detalle = "Seguimiento de evolucion clinica.",
            Anulado = false
        });

        // 7. EVENTOS REPRODUCTIVOS
        await AgregarSiFalta(db, Id(401), new EventoReproductivo
        {
            Id = Id(401),
            FichaBovinaId = lunaId,
            CuentaVeterinarioId = veterinarioId,
            Fecha = new DateTime(2026, 11, 11),
            Tipo = TipoEvento.DiagnosticoGestacion,
            Aptitud = Aptitud.Apta,
            Prenez = Prenez.Prenada,
            ProximoControl = new DateOnly(2026, 12, 10),
            PartoProbable = new DateOnly(2027, 5, 15),
            Detalle = "Gestacion confirmada en control veterinario.",
            Anulado = false
        });

        await AgregarSiFalta(db, Id(402), new EventoReproductivo
        {
            Id = Id(402),
            FichaBovinaId = moraId,
            CuentaVeterinarioId = veterinarioId,
            Fecha = new DateTime(2026, 11, 13),
            Tipo = TipoEvento.Valoracion,
            Aptitud = Aptitud.Pendiente,
            Prenez = Prenez.Desconocida,
            ProximoControl = new DateOnly(2026, 12, 13),
            Detalle = "Valoracion reproductiva pendiente de seguimiento.",
            Anulado = false
        });

        // 8. PREVENCIONES
        await AgregarSiFalta(db, prevencion1Id, new Prevencion
        {
            Id = prevencion1Id,
            FichaBovinaId = lunaId,
            CuentaVeterinarioId = veterinarioId,
            Tipo = TipoPrevencion.Control,
            Nombre = "Control sanitario general",
            FechaProgramada = new DateOnly(2026, 11, 18),
            Estado = EstadoPrevencion.Realizada,
            FechaRealizacion = new DateTime(2026, 11, 18, 9, 0, 0)
        });

        await AgregarSiFalta(db, prevencion2Id, new Prevencion
        {
            Id = prevencion2Id,
            FichaBovinaId = moraId,
            CuentaVeterinarioId = veterinarioId,
            Tipo = TipoPrevencion.Desparasitacion,
            Nombre = "Desparasitacion programada",
            FechaProgramada = new DateOnly(2026, 12, 5),
            Estado = EstadoPrevencion.Pendiente
        });

        await AgregarSiFalta(db, prevencion3Id, new Prevencion
        {
            Id = prevencion3Id,
            FichaBovinaId = lunaId,
            CuentaVeterinarioId = veterinarioId,
            Tipo = TipoPrevencion.Vacunacion,
            Nombre = "Vacunacion segun plan sanitario",
            FechaProgramada = new DateOnly(2026, 12, 15),
            Estado = EstadoPrevencion.Pendiente
        });

        await db.SaveChangesAsync();

        // 9. TRATAMIENTOS
        await AgregarSiFalta(db, tratamiento1Id, new Tratamiento
        {
            Id = tratamiento1Id,
            FichaBovinaId = lunaId,
            AtencionClinicaId = atencion1Id,
            CuentaVeterinarioId = veterinarioId,
            Medicamento = "Medicamento de prueba A",
            Dosis = 5,
            Unidad = "ml",
            Via = "Intramuscular",
            Frecuencia = "Cada 24 horas",
            Inicio = new DateOnly(2026, 11, 10),
            Duracion = "3 dias",
            FechaFin = new DateOnly(2026, 11, 12),
            Anulado = false
        });

        await AgregarSiFalta(db, tratamiento2Id, new Tratamiento
        {
            Id = tratamiento2Id,
            FichaBovinaId = moraId,
            AtencionClinicaId = atencion2Id,
            CuentaVeterinarioId = veterinarioId,
            Medicamento = "Medicamento de prueba B",
            Dosis = 2,
            Unidad = "ml",
            Via = "Subcutanea",
            Frecuencia = "Segun indicacion veterinaria",
            Inicio = new DateOnly(2026, 11, 12),
            Duracion = "2 dias",
            FechaFin = new DateOnly(2026, 11, 13),
            Anulado = false
        });

        await db.SaveChangesAsync();

        // 10. APLICACIONES
        await AgregarSiFalta(db, Id(701), new Aplicacion
        {
            Id = Id(701),
            FichaBovinaId = lunaId,
            CuentaTrabajadorId = trabajadorId,
            TratamientoId = tratamiento1Id,
            PrevencionId = null,
            FechaHoraReal = new DateTime(2026, 11, 10, 10, 0, 0),
            Dosis = 5,
            Unidad = "ml",
            Observacion = "Aplicacion de prueba registrada.",
            Tipo = "Tratamiento",
            VersionIndicacion = 1,
            Anulado = false
        });

        await AgregarSiFalta(db, Id(702), new Aplicacion
        {
            Id = Id(702),
            FichaBovinaId = lunaId,
            CuentaTrabajadorId = trabajadorId,
            TratamientoId = null,
            PrevencionId = prevencion1Id,
            FechaHoraReal = new DateTime(2026, 11, 18, 9, 0, 0),
            Dosis = 0,
            Unidad = "No aplica",
            Observacion = "Control sanitario realizado.",
            Tipo = "Prevencion",
            VersionIndicacion = 1,
            Anulado = false
        });

        await db.SaveChangesAsync();
    }
}
