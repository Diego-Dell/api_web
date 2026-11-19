# Integrante 3 · Tratamientos y Prevención

## Estructura de la carpeta:
- `Controllers/`: Colocar aquí los controladores (ej. `TratamientosController.cs`, `PrevencionController.cs`).
- `Models/`: Modelos y entidades de base de datos (ej. `Tratamiento.cs`, `PlanPrevencion.cs`).
- `Dtos/`: Objetos de transferencia de datos para solicitudes y respuestas (ej. `CrearTratamientoRequest.cs`, `ProgramarPrevencionRequest.cs`).
- `Endpoints/`: Mapeo de rutas si decides usar Minimal APIs en lugar de controladores.

## Alcance y Requisitos Asignados:
- **Tratamiento Médico:**
  - Debe estar vinculado obligatoriamente a una atención clínica del mismo animal.
  - Campos: Medicamento, dosis y unidad, vía de administración, frecuencia, fecha de inicio, duración y período de retiro (si corresponde).
- **Inmutabilidad y Versionado de Indicaciones:**
  - Modificar o finalizar conservando la indicación anterior: la versión vieja se cierra con `fecha_fin` y `motivo_fin`, y se crea una nueva indicación. **Nunca se sobrescribe**.
- **Prevención (Vacunas / Desparasitaciones / Controles):**
  - Programar indicación preventiva con: tipo, nombre y fecha programada. Estado inicial: `Pendiente`.
  - Al completar un control preventivo, el estado pasa a `Realizado`.
