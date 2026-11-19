# Integrante 4 · Cuidados y Aplicaciones

## Estructura de la carpeta:
- `Controllers/`: Colocar aquí los controladores (ej. `CuidadosController.cs`, `AplicacionesController.cs`).
- `Models/`: Modelos y entidades de base de datos (ej. `ObservacionCuidado.cs`, `AplicacionIndicacion.cs`, `CorreccionHistorial.cs`).
- `Dtos/`: Objetos de transferencia de datos para solicitudes y respuestas (ej. `RegistrarCuidadoRequest.cs`, `RegistrarAplicacionRequest.cs`).
- `Endpoints/`: Mapeo de rutas si decides usar Minimal APIs en lugar de controladores.

## Alcance y Requisitos Asignados:
- **Observación y Cuidado:**
  - Registro de lo que vio el trabajador y la acción realizada sobre el animal.
- **Aplicación Real de Indicación:**
  - Registro de la aplicación de un tratamiento o indicación preventiva del mismo animal.
  - Validación de dosis positiva (> 0).
- **Integración de Paneles:**
  - Conectar las opciones y accesos de los roles (panel de Veterinario y panel de Trabajador).
- **Ajustes y Requerimientos de Base de Datos:**
  - Automatización / Trigger para que al registrar la aplicación de vacuna o desparasitación, la indicación preventiva pase a estado `Realizada`.
  - Mecanismo para registrar correcciones auditables guardando el dato anterior y el motivo del cambio.
