# VetLink — Web API de Seguimiento Veterinario Bovino

## 1. Descripción del proyecto

**VetLink** es un sistema de seguimiento veterinario para granjas lecheras, orientado a centralizar la información sanitaria y reproductiva del ganado bovino mediante fichas individuales.

Esta Web API está desarrollada con ASP.NET Core y permite gestionar los registros del sistema mediante operaciones REST, utilizando SQL Server como base de datos relacional y Entity Framework Core para el acceso a datos y las migraciones.

El prototipo se basa en el diagrama de clases del proyecto VetLink e implementa 10 de sus 12 entidades principales, equivalentes al **83,3 % del modelo**.

## 2. Integrantes

| Nombre completo | Usuario GitHub |
|---|---|
| Walkyria Nicole Audivert Lino | waudivert |
| Matias Ruiz Decourt | Matias-Ruiz-Decourt |
| Diego Aron Dell Fernandez | Diego-Dell |
| Matías Marcelo Téllez Sengoku | MATUTETUPROX |

**Repositorio GitHub:** https://github.com/Diego-Dell/api_web

**Proyecto académico:** Ingeniería de Software — Universidad Privada del Valle.

## 3. Tecnologías utilizadas

- **Framework:** ASP.NET Core 10 (.NET 10)
- **Lenguaje:** C#
- **ORM:** Entity Framework Core 10
- **Base de datos:** Microsoft SQL Server
- **Documentación y pruebas:** Swagger UI (Swashbuckle)
- **Entorno de desarrollo:** Visual Studio 2026
- **Control de versiones:** Git y GitHub

## 4. Estructura del proyecto

```text
api_web/
├── Controllers/
│   └── Controladores REST
├── DTOs/
│   └── Objetos de entrada y salida
├── Models/
│   ├── Cuenta.cs
│   ├── Granja.cs
│   ├── Vinculacion.cs
│   ├── Invitacion.cs
│   ├── FichaBovina.cs
│   ├── AtencionClinica.cs
│   ├── EventoReproductivo.cs
│   ├── Tratamiento.cs
│   ├── Prevencion.cs
│   ├── Aplicacion.cs
│   └── Enums.cs
├── Data/
│   ├── AppDbContext.cs
│   └── DbInitializer.cs
├── Migrations/
│   └── Migraciones de Entity Framework Core
├── Properties/
│   └── launchSettings.json
├── Program.cs
├── appsettings.json
├── ApiWeb.csproj
├── ApiWeb.sln
└── README.md
```

La organización separa modelos, controladores, DTOs y acceso a datos para facilitar el mantenimiento y desarrollo colaborativo.

## 5. Modelo de datos

La API utiliza diez entidades relacionadas:

| Entidad | Descripción |
|---|---|
| Cuenta | Usuarios registrados en el sistema |
| Granja | Establecimientos ganaderos |
| Vinculacion | Relación de usuarios con granjas y roles |
| Invitacion | Invitaciones para incorporar miembros a una granja |
| FichaBovina | Identificación y datos individuales del ganado |
| AtencionClinica | Consultas, diagnósticos y seguimientos |
| EventoReproductivo | Registros de aptitud, gestación, partos y otros eventos |
| Tratamiento | Indicaciones y prescripciones veterinarias |
| Prevencion | Vacunaciones, desparasitaciones y controles programados |
| Aplicacion | Registro de aplicaciones y cuidados realizados |

Las relaciones se implementan mediante claves primarias `Guid` y claves foráneas configuradas con Entity Framework Core.

El sistema utiliza invitaciones para la incorporación de miembros, roles asociados a cada granja y bajas o anulaciones lógicas para conservar antecedentes cuando corresponde.

Las entidades `CambioRegistro` y `OperacionPendiente` quedan fuera del alcance de este prototipo.

## 6. Requisitos previos

Para ejecutar el proyecto se necesita:

- .NET SDK 10.
- Visual Studio compatible con .NET 10 o Visual Studio 2026.
- SQL Server o SQL Server LocalDB.
- Entity Framework Core CLI (`dotnet-ef` versión 10).
- Navegador web para acceder a Swagger.

Para comprobar la instalación:

```powershell
dotnet --version
dotnet ef --version
```

Si no está instalado `dotnet-ef`, ejecutar:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.15
```

## 7. Configuración de SQL Server

El proyecto utiliza la cadena `DefaultConnection` definida en `appsettings.json`.

**Cadena de conexión de ejemplo:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=VetLinkDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Esta configuración utiliza SQL Server LocalDB con autenticación de Windows y no contiene credenciales reales.

### Utilizar otra instancia SQL Server

Si se utiliza SQL Server Express o una instancia local diferente, se puede crear un archivo `appsettings.Development.json` con la conexión correspondiente:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=NOMBRE_DEL_SERVIDOR;Database=VetLinkDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Reemplazar `NOMBRE_DEL_SERVIDOR` por el nombre de la instancia local.

El archivo `appsettings.Development.json` se utiliza únicamente para la configuración personal y debe permanecer excluido del repositorio Git.

No es necesario crear manualmente las tablas de la base de datos.

## 8. Instalación y ejecución

**Paso 1. Clonar el repositorio**

```powershell
git clone https://github.com/Diego-Dell/api_web.git
cd api_web
```

**Paso 2. Restaurar dependencias**

```powershell
dotnet restore
```

**Paso 3. Compilar**

```powershell
dotnet build
```

**Paso 4. Aplicar migraciones**

```powershell
dotnet ef database update
```

Este comando crea o actualiza la base de datos `VetLinkDb` utilizando las migraciones incluidas en `Migrations/`.

**Paso 5. Ejecutar la API**

```powershell
dotnet run --launch-profile http
```

**Paso 6. Abrir Swagger UI**

Con la API ejecutándose, abrir:

http://localhost:5000/swagger

El puerto 5000 corresponde al perfil `http` configurado en `Properties/launchSettings.json`.

Para detener la aplicación, presionar `Ctrl + C` en la terminal.

## 9. Datos de prueba

La aplicación incluye `Data/DbInitializer.cs`, que carga automáticamente datos ficticios cuando se ejecuta en el entorno `Development`.

Los datos se insertan después de aplicar las migraciones, al iniciar la API.

| Entidad | Registros iniciales |
|---|---:|
| Cuentas | 3 |
| Granjas | 2 |
| Vinculaciones | 3 |
| Invitaciones | 2 |
| FichasBovinas | 3 |
| AtencionesClinicas | 3 |
| EventosReproductivos | 2 |
| Tratamientos | 2 |
| Prevenciones | 3 |
| Aplicaciones | 2 |
| **Total** | **25** |

### Granjas de prueba

| Granja | Identificador |
|---|---|
| Hacienda Demo | 11111111-1111-1111-1111-111111111111 |
| Granja Los Pinos | 22222222-2222-2222-2222-222222222222 |

### Fichas bovinas de prueba

| Identificación | Nombre | Etapa | Granja |
|---|---|---|---|
| A-001 | Luna | Vaca | Hacienda Demo |
| A-002 | Mora | Vaquilla | Hacienda Demo |
| B-001 | Estrella | Ternera | Granja Los Pinos |

Los registros utilizan identificadores GUID fijos para mantener sus relaciones y facilitar las pruebas.

Antes de insertar cada registro, el inicializador comprueba si ya existe por su identificador, evitando duplicar esos datos al reiniciar la aplicación.

Los datos clínicos, tratamientos y aplicaciones son ficticios y se utilizan exclusivamente para comprobar el funcionamiento informático del prototipo.

## 10. Operaciones de la API

Los controladores REST están destinados a proporcionar las operaciones CRUD de las diez entidades implementadas.

| Método HTTP | Operación | Respuesta esperada |
|---|---|---|
| GET | Listar registros | 200 OK |
| GET por ID | Consultar un registro | 200 OK / 404 Not Found |
| POST | Crear un registro | 201 Created y Location |
| PUT | Actualizar un registro | 204 No Content / 404 Not Found |
| DELETE | Eliminar, desactivar o anular según la entidad | 204 No Content / 404 Not Found |

También se contemplan consultas de recursos relacionados, como fichas de una granja o atenciones clínicas de un animal.

Las operaciones deben respetar las claves foráneas y validar campos obligatorios, reglas del dominio y referencias inexistentes.

## 11. Validaciones y manejo de errores

La API debe comprobar:

- Campos obligatorios.
- Identificación bovina única dentro de cada granja.
- Existencia de registros relacionados.
- Valores válidos para estados, etapas y roles.
- Dosis positivas cuando corresponda.
- Coherencia entre animal, atención clínica, tratamiento y aplicación.
- Conservación de antecedentes mediante bajas y anulaciones lógicas.

Las solicitudes inválidas deben producir respuestas HTTP adecuadas, incluyendo:

- **400 Bad Request:** datos inválidos o referencias no permitidas.
- **404 Not Found:** recurso solicitado inexistente.

## 12. Persistencia de datos

Los datos se almacenan de forma persistente en SQL Server mediante Entity Framework Core.

Para comprobarlo:

1. Ejecutar la API y abrir Swagger.
2. Crear un registro mediante POST.
3. Confirmar la respuesta HTTP 201 y guardar el ID generado.
4. Detener la aplicación con `Ctrl + C`.
5. Ejecutarla nuevamente.
6. Consultar el registro mediante GET utilizando su ID.
7. Comprobar la respuesta HTTP 200 y la conservación de la información.

La persistencia depende de SQL Server y no de almacenamiento temporal en memoria.

## 13. Evidencias de pruebas

El archivo PDF de evidencias de la entrega debe incluir:

- Captura general de Swagger UI con los endpoints.
- Pruebas exitosas GET, POST, PUT y DELETE.
- Respuesta 201 Created con cabecera Location.
- Prueba de validación con respuesta 400 Bad Request.
- Consulta inexistente con respuesta 404 Not Found.
- Consulta de un recurso relacionado.
- Prueba GET después de reiniciar la API para demostrar persistencia.

Cada evidencia debe identificar la operación realizada y el código HTTP obtenido.

## 14. Control de versiones y colaboración

El desarrollo se realiza en un repositorio GitHub compartido.

Los integrantes trabajan mediante ramas independientes, realizando commits atribuibles a sus cuentas de GitHub.

La integración se realiza mediante Pull Requests, conservando el historial de cambios y los aportes individuales al desarrollo, las pruebas y la documentación.

## 15. Alcance del prototipo

Esta API tiene como objetivo demostrar el modelo relacional y las operaciones REST del sistema VetLink.

No incluye en esta entrega:

- Integración con Flutter.
- Sincronización sin conexión.
- Despliegue en Render.
- Integración con Supabase.
- Auditoría completa de versiones.
- Implementación de todas las funcionalidades del sistema final.

El prototipo utiliza SQL Server para facilitar la ejecución local y las pruebas mediante Swagger.