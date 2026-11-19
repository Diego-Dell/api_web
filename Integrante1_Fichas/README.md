# Integrante 1 · Fichas de Ganado

> **Nota:** Este módulo es prioritario porque los demás integrantes dependen de la existencia de las fichas de ganado.

## Qué hace

Gestiona las fichas de la tabla `fichas_bovinas` de Supabase, la misma que usa la app móvil. No crea una tabla nueva: `FichaGanadoConfiguracion` la mapea y la excluye de las migraciones de EF.

| Método | Ruta | Quién puede |
|---|---|---|
| GET | `api/fichas/granja/{granjaId}` | Cualquier miembro activo de la granja. Agrega `?conBajas=true` para incluir los animales dados de baja |
| GET | `api/fichas/{id}` | Cualquier miembro activo de la granja del animal, también si está dado de baja |
| POST | `api/fichas` | Dueño o trabajador |
| PUT | `api/fichas/{id}` | Dueño o trabajador, solo si el animal no está dado de baja |
| POST | `api/fichas/{id}/baja` | Solo el dueño |

Hay ejemplos de cada llamada en `ApiWeb.http`.

## Autenticación

Todas las rutas piden el `access_token` de la sesión de Supabase en la cabecera `Authorization: Bearer`. La API valida el token con las claves públicas del proyecto (`Supabase:Url` en `appsettings.json` o la variable `SUPABASE_URL`) y toma la cuenta del campo `sub`.

La API se conecta a Postgres con un usuario que no pasa por las políticas RLS. Por eso `FichasServicio.RolEnGranja` repite la regla de `rol_en_granja`: vínculo activo, cuenta activa y granja confirmada.

## Reglas

- La identificación es obligatoria y no se repite dentro de una granja; en otra granja sí se acepta.
- Nombre y raza son opcionales; los textos vacíos se guardan como `null`.
- La etapa es `ternera`, `vaquilla` o `vaca`.
- La fecha de nacimiento no puede ser futura, y "estimada" solo vale si hay fecha.
- La baja necesita fecha y motivo; la fecha no puede ser futura ni anterior al nacimiento.
- Un animal dado de baja sigue consultable, pero no se edita ni admite eventos nuevos. La base lo impide con el trigger `sin_eventos_tras_baja`.

## Para los demás integrantes

- El id de una ficha es un `Guid` (uuid en la base), no un `int`. Usen `Guid` en su campo del animal.
- Antes de registrar un evento, llamen a `FichasServicio.AdmiteEventos(fichaId)` para responder con un mensaje claro si el animal está dado de baja.
- `FichasServicio.RolEnGranja(cuentaId, granjaId)` les sirve para comprobar el rol en sus propios controladores.

## Arreglo en la base

En Supabase, las políticas `fichas_crear` y `fichas_editar` todavía dejan entrar al dueño y al veterinario, pero el RF-02 dice dueño y trabajador. El script `2026-11-19_fichas_dueno_trabajador.sql` (fuera de este repositorio) las corrige. La API ya aplica la regla correcta por su cuenta, pero la app móvil depende de esas políticas.
