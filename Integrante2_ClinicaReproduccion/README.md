# Integrante 2 · Clínica y Reproducción

## Estructura de la carpeta:
- `Controllers/`: Colocar aquí los controladores (ej. `AtencionesController.cs`, `ReproduccionController.cs`).
- `Models/`: Modelos y entidades de base de datos (ej. `AtencionClinica.cs`, `EventoReproduccion.cs`).
- `Dtos/`: Objetos de transferencia de datos para solicitudes y respuestas (ej. `RegistrarAtencionRequest.cs`, `EstadoReproductivoDto.cs`).
- `Endpoints/`: Mapeo de rutas si decides usar Minimal APIs en lugar de controladores.

## Alcance y Requisitos Asignados:
- **Atención Clínica:**
  - Diagnóstico, control o evolución del animal, con fecha y detalle clínico.
- **Reproducción:**
  - **Aptitud reproductiva:** `Sin evaluar`, `Todavía no apta`, `Apta`.
  - **Estado de preñez:** `No determinada`, `No gestante`, `Confirmada`.
  - **Parto o pérdida:** Registro con próximo control y fecha probable de parto (FPP).
- **Antecedentes del Animal:**
  - Consulta de listas históricas de atenciones médicas y eventos reproductivos del animal.
- **Reglas de Acceso / Base de Datos:**
  - Según RF-03: "Solo el veterinario valora y programa", pero revisar políticas si el dueño también tiene permisos.
