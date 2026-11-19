# Base de Datos Compartida (Supabase / PostgreSQL)

En esta carpeta se encuentra `AppDbContext.cs`, el cual es el contexto compartido de Entity Framework Core para conectarse a **Supabase**.

## Instrucciones para los integrantes:
1. **Modelos:** Cada integrante define sus entidades/modelos dentro de su propia carpeta `Models/`.
2. **Registro de DbSet:** Cada integrante debe agregar su `DbSet<MiEntidad>` en `AppDbContext.cs`.
3. **Migraciones:**
   - Para crear una nueva migración:
     ```bash
     dotnet ef migrations add NombreMigracion
     ```
   - Para aplicar la migración a la base de datos de Supabase:
     ```bash
     dotnet ef database update
     ```
