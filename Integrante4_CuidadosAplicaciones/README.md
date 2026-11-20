# Integrante 4: cuidados y aplicaciones

Módulo de la Web API compartida `ApiWeb` (.NET 10). Usa las tablas existentes de Supabase
`fichas_bovinas`, `tratamientos`, `prevenciones` y `aplicaciones`. Los identificadores son UUID,
como en la base del equipo. No usa ni crea la base SQLite del prototipo anterior.

## Configuración

1. Abrir `ApiWeb.sln` en Visual Studio 2022 con .NET 10 instalado.
2. La migración `migrations/20261119_cuidados_aplicaciones.sql` ya fue aplicada al
   Supabase compartido. Conservarla para recrear el esquema en otra instancia. Crea
   `cuidados`, `correcciones_registros` y el trigger que marca la prevención como
   `realizada`; no recrea las tablas de otros integrantes.
3. Configurar la cadena privada como variable de entorno local `ConnectionStrings__SupabaseConnection`
   o `DATABASE_URL`. No guardarla en Git ni en el ZIP. Ejemplo sin contraseña real:
   `Host=HOST_DEL_POOLER;Port=5432;Database=postgres;Username=postgres.PROJECT_REF;Password=CAMBIAR;SSL Mode=Require`
4. Iniciar la API y abrir `/swagger`. Para las rutas de este módulo usar el token de
   Supabase Auth en `Authorization: Bearer <access_token>`. El `sub` del token identifica
   a la cuenta que registró o corrigió el dato.

La conexión PostgreSQL del equipo está en `Program.cs`. Una URL y una clave pública de
Supabase no sustituyen la contraseña de PostgreSQL usada por Npgsql. Por eso abrir el ZIP
en otra computadora requiere configurar la variable de entorno, o bien probar la API ya
desplegada por el equipo.

## Rutas

| Operación | Ruta | Resultado |
| --- | --- | --- |
| GET | `/api/cuidados/animal/{fichaId}` | Lista cuidados activos del animal |
| GET | `/api/cuidados/{id}` | Detalle, incluso si fue anulado |
| POST | `/api/cuidados` | 201 y cabecera Location |
| PUT | `/api/cuidados/{id}` | Corrige y guarda valores anteriores y motivo |
| DELETE | `/api/cuidados/{id}?motivo=...` | Anula sin borrar el historial; 204 |
| GET | `/api/cuidados/{id}/correcciones` | Historial |
| GET | `/api/aplicaciones/animal/{fichaId}` | Lista aplicaciones activas |
| GET | `/api/aplicaciones/{id}` | Detalle |
| POST | `/api/aplicaciones` | 201 y cabecera Location |
| PUT | `/api/aplicaciones/{id}` | Corrige dosis, unidad u observación con motivo |
| DELETE | `/api/aplicaciones/{id}?motivo=...` | Anula; 204 |
| GET | `/api/aplicaciones/{id}/correcciones` | Historial |

Con `?conAnulados=true` o `?conAnuladas=true` las listas incluyen registros anulados.
Las referencias inexistentes o de otro animal producen 400/404. Una dosis no positiva
produce 400. Un animal dado de baja produce 409. Todo el módulo exige token válido y
vinculación activa a una granja confirmada. El registro de cuidados está limitado a
dueño y trabajador.

## Pruebas recomendadas en Swagger

1. Crear o identificar una ficha activa, un tratamiento o una prevención pendiente.
2. POST aplicación con `fichaId`, exactamente uno de `tratamientoId` o `prevencionId`,
   `dosis` positiva y `unidad`. Comprobar 201 y Location.
3. Consultar GET por ID y por animal. Si fue preventiva, comprobar en Supabase que
   `prevenciones.estado` pasó a `realizada`.
4. Probar POST con dosis 0 y con referencia de otro animal: 400.
5. Probar GET con UUID inexistente: 404.
6. PUT con motivo y comprobar GET de correcciones. DELETE con motivo y comprobar
   que el registro se puede consultar con `conAnuladas=true`.
7. Reiniciar la API y repetir GET: los datos deben continuar en PostgreSQL.

El archivo de migración se incluye en Git para que los cambios de esquema sean
reproducibles. El historial de commits debe mostrar el aporte personal del integrante.
