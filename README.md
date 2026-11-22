# VetLink — Web API de Seguimiento Veterinario Bovino

## 1. Descripción del proyecto

**VetLink** es un sistema de seguimiento veterinario para granjas lecheras, orientado a centralizar la información sanitaria y reproductiva del ganado bovino mediante fichas individuales.

Esta Web API está desarrollada con ASP.NET Core y permite gestionar los registros del sistema mediante operaciones REST, utilizando PostgreSQL de Supabase (la misma base central de la app VetLink) y Entity Framework Core para el acceso a datos.

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
- **Base de datos:** PostgreSQL en Supabase (Npgsql)
- **Autenticación:** tokens de Supabase Auth (JWT) y políticas RLS
- **Despliegue:** Render (Docker)
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
│   └── SesionSupabase.cs
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

- .NET SDK 10.
- Un proyecto de Supabase con el esquema de VetLink (`schema.sql`) y el script `2026-11-22_api_web.sql` aplicados.
- Una cuenta de la app VetLink para obtener un token de acceso.

## 7. Conexión con Supabase

La API no crea tablas ni usa migraciones: trabaja sobre las tablas de Supabase.

| Variable | Valor |
|---|---|
| `SUPABASE_URL` | URL del proyecto, por ejemplo `https://xxxx.supabase.co` |
| `DATABASE_URL` | Cadena **Session pooler** de Supabase (Connect > Session pooler), con la contraseña de la base |

Para ejecutar en local, crear `appsettings.Development.json` (no se sube al repositorio):

```json
{
  "Supabase": { "Url": "https://xxxx.supabase.co" },
  "ConnectionStrings": { "Supabase": "postgresql://postgres.xxxx:CONTRASEÑA@aws-0-sa-east-1.pooler.supabase.com:5432/postgres" }
}
```

### Seguridad

Cada petición lleva `Authorization: Bearer <access_token>` de Supabase. La API valida el token con las llaves públicas del proyecto y ejecuta las consultas como el rol `authenticated` de esa persona, así se aplican las mismas políticas RLS que en la app: nadie ve ni modifica datos de granjas ajenas.

Las cuentas y contraseñas las maneja Supabase Auth; la API solo consulta cuentas y edita el nombre.

## 8. Instalación y ejecución

```powershell
dotnet restore
dotnet run --launch-profile http
```

Swagger queda en `http://localhost:5000/swagger`. Usar **Authorize** y pegar el token de acceso.

### Despliegue en Render

1. En Render: **New > Blueprint** y elegir este repositorio (usa `render.yaml` y el `Dockerfile`).
2. Completar `SUPABASE_URL` y `DATABASE_URL` cuando Render los pida.
3. Al terminar, `https://<servicio>.onrender.com/health` responde `{"estado":"ok"}` y `/swagger` muestra los endpoints.

El plan gratuito de Render se suspende sin uso; la primera petición después de un rato tarda unos segundos.

## 9. Datos

Los datos son los de la app VetLink en Supabase. Para probar, crear una granja y fichas desde la app o desde Swagger con una cuenta que pertenezca a la granja.

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
- **401 Unauthorized:** falta el token de Supabase o no es válido.
- **403 Forbidden:** el rol de la persona en esa granja no permite la operación.
- **404 Not Found:** recurso solicitado inexistente.
- **409 Conflict:** registro repetido.

## 12. Persistencia de datos

Los datos se almacenan de forma persistente en PostgreSQL de Supabase mediante Entity Framework Core.

Para comprobarlo:

1. Ejecutar la API y abrir Swagger.
2. Crear un registro mediante POST.
3. Confirmar la respuesta HTTP 201 y guardar el ID generado.
4. Detener la aplicación con `Ctrl + C`.
5. Ejecutarla nuevamente.
6. Consultar el registro mediante GET utilizando su ID.
7. Comprobar la respuesta HTTP 200 y la conservación de la información.

La persistencia depende de Supabase y no de almacenamiento temporal en memoria.

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

La API usa la misma base de Supabase que la app, sin duplicar datos en otra base.