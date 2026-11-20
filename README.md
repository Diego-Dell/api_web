# VetLink - Proyecto Web API (4 Integrantes)

Proyecto base desarrollado en **ASP.NET Core (.NET 10)** estructurado para el trabajo colaborativo en equipo de 4 integrantes, con soporte para controladores y Minimal APIs, base de datos en **Supabase (PostgreSQL)** y despliegue en **Render**.

---

## 👥 Distribución del Equipo por Carpetas

Cada integrante cuenta con su propia subcarpeta independiente para evitar conflictos de Git y mantener una separación de responsabilidades clara:

```
ApiWeb/
├── Integrante1_Fichas/                 --> Fichas de Ganado (Base para los demás módulos)
│   ├── Controllers/                    --> Controladores de API
│   ├── Models/                         --> Entidades y modelos
│   ├── Dtos/                           --> Requests / Responses DTOs
│   ├── Endpoints/                      --> Minimal APIs (opcional)
│   └── README.md                       --> Requisitos específicos del Integrante 1
│
├── Integrante2_ClinicaReproduccion/    --> Clínica y Reproducción
│   ├── Controllers/
│   ├── Models/
│   ├── Dtos/
│   ├── Endpoints/
│   └── README.md                       --> Requisitos específicos del Integrante 2
│
├── Integrante3_TratamientosPrevencion/ --> Tratamientos y Prevención
│   ├── Controllers/
│   ├── Models/
│   ├── Dtos/
│   ├── Endpoints/
│   └── README.md                       --> Requisitos específicos del Integrante 3
│
└── Integrante4_CuidadosAplicaciones/   --> Cuidados y Aplicaciones
    ├── Controllers/
    ├── Models/
    ├── Dtos/
    ├── Endpoints/
    └── README.md                       --> Requisitos específicos del Integrante 4
```

---

## 🗄️ Base de Datos Compartida (`Data/AppDbContext.cs`)

- Todo el equipo comparte el mismo contexto de Entity Framework Core ubicado en `Data/AppDbContext.cs`.
- Cuando un integrante cree una nueva entidad en su carpeta `Models/`, debe agregar la línea de su `DbSet` en `AppDbContext.cs`:
  ```csharp
  public DbSet<MiModelo> MisModelos => Set<MiModelo>();
  ```

---

## ⚙️ Configuración con Supabase (PostgreSQL)

Configurar la cadena privada como variable de entorno `ConnectionStrings__SupabaseConnection`
o `DATABASE_URL`. No guardar la contraseña en `appsettings.json` ni subirla a Git.
Ejemplo sin contraseña real:

```json
"ConnectionStrings": {
  "SupabaseConnection": "Host=HOST_DEL_POOLER;Port=5432;Database=postgres;Username=postgres.PROJECT_REF;Password=CAMBIAR;SSL Mode=Require"
}
```

> **Consejo:** Supabase ofrece la cadena en modo **Transaction Pooler (puerto 6543)** o **Session Pooler (puerto 5432)** en la sección *Project Settings > Database > Connection string > URI / .NET*.

---

## 🚀 Despliegue en Render

1. El repositorio incluye un `Dockerfile` optimizado y un archivo `render.yaml`.
2. En Render, crear un nuevo **Web Service** conectado a este repositorio.
3. En la sección **Environment Variables**, agregar:
   - `DATABASE_URL`: Cadena de conexión completa de Supabase.
   - `ASPNETCORE_ENVIRONMENT`: `Production`
4. Render ejecutará el contenedor y el endpoint de salud estará disponible en `/health` y la documentación interactiva en `/swagger`.

---

## 💻 Cómo Ejecutar el Proyecto en Visual Studio

1. Abrir el archivo `ApiWeb.sln` (o `ApiWeb.slnx`) con **Visual Studio 2022**.
2. Presionar **F5** o hacer clic en **Iniciar depuración** (perfil `https` o `http`).
3. El navegador abrirá automáticamente la interfaz interactiva de **Swagger UI** (`http://localhost:5000/` o `https://localhost:7001/`).

El módulo del Integrante 4 usa las tablas existentes de Supabase y su migración está
en `Integrante4_CuidadosAplicaciones/migrations/`. Ya se aplicó al proyecto compartido.
Las rutas de cuidados y aplicaciones requieren un token de Supabase Auth, que se
ingresa con el botón **Authorize** de Swagger.
