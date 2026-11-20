-- Ejecutar una vez en Supabase SQL Editor, sobre el proyecto VetLink.
-- No recrea aplicaciones, fichas_bovinas, tratamientos ni prevenciones.
begin;

create table if not exists public.cuidados (
    id uuid primary key default gen_random_uuid(),
    ficha_id uuid not null references public.fichas_bovinas(id) on delete restrict,
    registrado_por uuid not null references public.cuentas(id) on delete restrict,
    fecha_hora timestamptz not null default now(),
    observacion text not null check (length(btrim(observacion)) > 0),
    accion_realizada text not null check (length(btrim(accion_realizada)) > 0),
    anulado boolean not null default false
);
create index if not exists ix_cuidados_ficha_fecha on public.cuidados(ficha_id, fecha_hora desc);

create table if not exists public.correcciones_registros (
    id uuid primary key default gen_random_uuid(),
    tabla text not null check (tabla in ('cuidados', 'aplicaciones')),
    registro_id uuid not null,
    campo text not null check (length(btrim(campo)) > 0),
    valor_anterior text,
    valor_nuevo text,
    motivo text not null check (length(btrim(motivo)) > 0),
    corregido_por uuid not null references public.cuentas(id) on delete restrict,
    fecha_hora timestamptz not null default now()
);
create index if not exists ix_correcciones_registro on public.correcciones_registros(tabla, registro_id, fecha_hora);
create unique index if not exists ux_aplicaciones_prevencion_activa
    on public.aplicaciones(prevencion_id) where prevencion_id is not null and not anulado;

-- La API usa una conexión PostgreSQL y comprueba el rol del token de Supabase.
-- Las tablas nuevas no se exponen directamente a las claves anon/authenticated.
alter table public.cuidados enable row level security;
alter table public.correcciones_registros enable row level security;

create or replace function public.sincronizar_prevencion_aplicacion()
returns trigger
language plpgsql
set search_path = ''
as $$
begin
    if new.prevencion_id is not null and new.anulado = false then
        update public.prevenciones
           set estado = 'realizada'::public.estado_prevencion
         where id = new.prevencion_id and ficha_id = new.ficha_id
           and estado = 'pendiente'::public.estado_prevencion;
    elsif tg_op = 'UPDATE' and old.prevencion_id is not null
          and old.anulado = false and new.anulado = true then
        update public.prevenciones
           set estado = 'pendiente'::public.estado_prevencion
         where id = old.prevencion_id
           and estado = 'realizada'::public.estado_prevencion
           and not exists (
               select 1 from public.aplicaciones a
                where a.prevencion_id = old.prevencion_id and not a.anulado
           );
    end if;
    return new;
end;
$$;

do $trigger$
begin
    if not exists (
        select 1 from pg_trigger
        where tgrelid = 'public.aplicaciones'::regclass
          and tgname = 'sincronizar_prevencion_aplicacion'
    ) then
        create trigger sincronizar_prevencion_aplicacion
        after insert or update of anulado on public.aplicaciones
        for each row execute function public.sincronizar_prevencion_aplicacion();
    end if;
end;
$trigger$;

commit;
