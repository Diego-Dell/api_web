namespace ApiWeb.Models;

// 1. Mismos valores que los tipos enumerados de Supabase (public.etapa, public.tipo_atencion, ...).
public enum Etapa { Ternera, Vaquilla, Vaca }
public enum TipoAtencion { Consulta, Diagnostico, Control, Emergencia }
public enum TipoEvento { Celo, Inseminacion, Valoracion, Parto, Perdida, Secado }
public enum Aptitud { Apta, NoApta, EnObservacion }
public enum Prenez { Prenada, Vacia, NoEvaluada }
public enum TipoPrevencion { Vacuna, Desparasitacion, Control }
public enum EstadoPrevencion { Pendiente, Realizada, Cancelada }

// 2. Roles guardados en Supabase; la API los expone como Propietario, Veterinario y Trabajador.
public enum RolGranja { Dueno, Veterinario, Trabajador }
public enum RolInvitado { Veterinario, Trabajador }
