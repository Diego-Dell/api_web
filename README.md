# VetLink — Fase 1

Base de la Web API de VetLink sobre ASP.NET Core .NET 10, Entity Framework Core 10 y SQL Server LocalDB.

## Estructura

```text
ApiWeb.sln
ApiWeb.csproj
Controllers/      (CRUD de fase 2)
DTOs/             (DTOs de fase 2)
Data/AppDbContext.cs
Migrations/       (migraciones EF Core)
Models/           (diez entidades del diagrama)
Program.cs
appsettings.json
```

Las clases `CambioRegistro` y `OperacionPendiente` quedan para una fase posterior. Los roles se asignan mediante `Vinculacion`; el acceso se inicia por invitaciones. Las bajas y anulaciones son campos de estado para preservar historial. No se modelan códigos de acceso ni períodos de retiro.

## Requisitos

- .NET 10 SDK
- SQL Server LocalDB (`MSSQLLocalDB`)
- Herramienta `dotnet-ef` 10

La cadena de conexión de ejemplo está en `appsettings.json` y no contiene credenciales reales. Puede sustituirse con `ConnectionStrings:DefaultConnection` en el entorno local.

## Ejecutar

```powershell
dotnet restore
dotnet build
dotnet ef database update
dotnet run --launch-profile http
```

Con el perfil `http`, Swagger UI se abre en `http://localhost:5000/swagger`. Esta fase no incluye controladores CRUD ni DTOs de negocio.
