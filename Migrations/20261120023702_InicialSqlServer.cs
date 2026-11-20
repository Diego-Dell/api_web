using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiWeb.Migrations
{
    /// <inheritdoc />
    public partial class InicialSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cuentas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Identificador = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClaveHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    AdministradorTecnico = table.Column<bool>(type: "bit", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuentas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Granjas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Hectareas = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Trabajadores = table.Column<int>(type: "int", nullable: false),
                    DocumentosTerreno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RUNSA = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Confirmada = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Granjas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FichasBovinas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GranjaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Identificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Raza = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Etapa = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nacimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    NacimientoEstimado = table.Column<bool>(type: "bit", nullable: false),
                    FechaBaja = table.Column<DateOnly>(type: "date", nullable: true),
                    MotivoBaja = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichasBovinas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FichasBovinas_Granjas_GranjaId",
                        column: x => x.GranjaId,
                        principalTable: "Granjas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invitaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GranjaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmitidaPorCuentaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Vigente = table.Column<bool>(type: "bit", nullable: false),
                    Aceptada = table.Column<bool>(type: "bit", nullable: false),
                    CorreoDestinatario = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invitaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invitaciones_Cuentas_EmitidaPorCuentaId",
                        column: x => x.EmitidaPorCuentaId,
                        principalTable: "Cuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invitaciones_Granjas_GranjaId",
                        column: x => x.GranjaId,
                        principalTable: "Granjas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vinculaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GranjaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vinculaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vinculaciones_Cuentas_CuentaId",
                        column: x => x.CuentaId,
                        principalTable: "Cuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vinculaciones_Granjas_GranjaId",
                        column: x => x.GranjaId,
                        principalTable: "Granjas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AtencionesClinicas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FichaBovinaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaVeterinarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Detalle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Anulado = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtencionesClinicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AtencionesClinicas_Cuentas_CuentaVeterinarioId",
                        column: x => x.CuentaVeterinarioId,
                        principalTable: "Cuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AtencionesClinicas_FichasBovinas_FichaBovinaId",
                        column: x => x.FichaBovinaId,
                        principalTable: "FichasBovinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventosReproductivos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FichaBovinaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaVeterinarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Aptitud = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Prenez = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ProximoControl = table.Column<DateOnly>(type: "date", nullable: true),
                    PartoProbable = table.Column<DateOnly>(type: "date", nullable: true),
                    NumeroParto = table.Column<int>(type: "int", nullable: true),
                    Detalle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosReproductivos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventosReproductivos_Cuentas_CuentaVeterinarioId",
                        column: x => x.CuentaVeterinarioId,
                        principalTable: "Cuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventosReproductivos_FichasBovinas_FichaBovinaId",
                        column: x => x.FichaBovinaId,
                        principalTable: "FichasBovinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Prevenciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FichaBovinaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaVeterinarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaProgramada = table.Column<DateOnly>(type: "date", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaRealizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prevenciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prevenciones_Cuentas_CuentaVeterinarioId",
                        column: x => x.CuentaVeterinarioId,
                        principalTable: "Cuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prevenciones_FichasBovinas_FichaBovinaId",
                        column: x => x.FichaBovinaId,
                        principalTable: "FichasBovinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tratamientos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FichaBovinaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AtencionClinicaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaVeterinarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Medicamento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dosis = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Via = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Frecuencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    Duracion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    MotivoFin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tratamientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tratamientos_AtencionesClinicas_AtencionClinicaId",
                        column: x => x.AtencionClinicaId,
                        principalTable: "AtencionesClinicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tratamientos_Cuentas_CuentaVeterinarioId",
                        column: x => x.CuentaVeterinarioId,
                        principalTable: "Cuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tratamientos_FichasBovinas_FichaBovinaId",
                        column: x => x.FichaBovinaId,
                        principalTable: "FichasBovinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Aplicaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FichaBovinaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaTrabajadorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TratamientoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PrevencionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaHoraReal = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Dosis = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Anulado = table.Column<bool>(type: "bit", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VersionIndicacion = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aplicaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aplicaciones_Cuentas_CuentaTrabajadorId",
                        column: x => x.CuentaTrabajadorId,
                        principalTable: "Cuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Aplicaciones_FichasBovinas_FichaBovinaId",
                        column: x => x.FichaBovinaId,
                        principalTable: "FichasBovinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Aplicaciones_Prevenciones_PrevencionId",
                        column: x => x.PrevencionId,
                        principalTable: "Prevenciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Aplicaciones_Tratamientos_TratamientoId",
                        column: x => x.TratamientoId,
                        principalTable: "Tratamientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aplicaciones_CuentaTrabajadorId",
                table: "Aplicaciones",
                column: "CuentaTrabajadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Aplicaciones_FichaBovinaId",
                table: "Aplicaciones",
                column: "FichaBovinaId");

            migrationBuilder.CreateIndex(
                name: "IX_Aplicaciones_PrevencionId",
                table: "Aplicaciones",
                column: "PrevencionId");

            migrationBuilder.CreateIndex(
                name: "IX_Aplicaciones_TratamientoId",
                table: "Aplicaciones",
                column: "TratamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_AtencionesClinicas_CuentaVeterinarioId",
                table: "AtencionesClinicas",
                column: "CuentaVeterinarioId");

            migrationBuilder.CreateIndex(
                name: "IX_AtencionesClinicas_FichaBovinaId",
                table: "AtencionesClinicas",
                column: "FichaBovinaId");

            migrationBuilder.CreateIndex(
                name: "IX_Cuentas_Identificador",
                table: "Cuentas",
                column: "Identificador",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventosReproductivos_CuentaVeterinarioId",
                table: "EventosReproductivos",
                column: "CuentaVeterinarioId");

            migrationBuilder.CreateIndex(
                name: "IX_EventosReproductivos_FichaBovinaId",
                table: "EventosReproductivos",
                column: "FichaBovinaId");

            migrationBuilder.CreateIndex(
                name: "IX_FichasBovinas_GranjaId_Identificacion",
                table: "FichasBovinas",
                columns: new[] { "GranjaId", "Identificacion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invitaciones_EmitidaPorCuentaId",
                table: "Invitaciones",
                column: "EmitidaPorCuentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Invitaciones_GranjaId",
                table: "Invitaciones",
                column: "GranjaId");

            migrationBuilder.CreateIndex(
                name: "IX_Prevenciones_CuentaVeterinarioId",
                table: "Prevenciones",
                column: "CuentaVeterinarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Prevenciones_FichaBovinaId",
                table: "Prevenciones",
                column: "FichaBovinaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tratamientos_AtencionClinicaId",
                table: "Tratamientos",
                column: "AtencionClinicaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tratamientos_CuentaVeterinarioId",
                table: "Tratamientos",
                column: "CuentaVeterinarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Tratamientos_FichaBovinaId",
                table: "Tratamientos",
                column: "FichaBovinaId");

            migrationBuilder.CreateIndex(
                name: "IX_Vinculaciones_CuentaId_GranjaId",
                table: "Vinculaciones",
                columns: new[] { "CuentaId", "GranjaId" },
                unique: true,
                filter: "[Activa] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Vinculaciones_GranjaId",
                table: "Vinculaciones",
                column: "GranjaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aplicaciones");

            migrationBuilder.DropTable(
                name: "EventosReproductivos");

            migrationBuilder.DropTable(
                name: "Invitaciones");

            migrationBuilder.DropTable(
                name: "Vinculaciones");

            migrationBuilder.DropTable(
                name: "Prevenciones");

            migrationBuilder.DropTable(
                name: "Tratamientos");

            migrationBuilder.DropTable(
                name: "AtencionesClinicas");

            migrationBuilder.DropTable(
                name: "Cuentas");

            migrationBuilder.DropTable(
                name: "FichasBovinas");

            migrationBuilder.DropTable(
                name: "Granjas");
        }
    }
}
