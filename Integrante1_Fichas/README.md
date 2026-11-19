# Integrante 1 · Fichas de Ganado

> **Nota:** Este módulo es prioritario porque los demás integrantes dependen de la existencia de las fichas de ganado.

## Estructura de la carpeta:
- `Controllers/`: Colocar aquí los controladores (ej. `FichasController.cs`).
- `Models/`: Modelos y entidades de base de datos (ej. `FichaGanado.cs`, `BajaGanado.cs`).
- `Dtos/`: Objetos de transferencia de datos para solicitudes y respuestas (ej. `CrearFichaRequest.cs`, `FichaDetalleDto.cs`).
- `Endpoints/`: Mapeo de rutas si decides usar Minimal APIs en lugar de controladores.

## Alcance y Requisitos Asignados:
- **Lista y Detalle de Ganado:**
  - Lista del ganado de la granja actual.
  - Detalle de la ficha del animal (donde los demás módulos conectarán sus acciones y consultas).
- **Crear y Editar Ficha:**
  - Identificación obligatoria.
  - Nombre y raza opcionales.
  - Etapa productiva / categoría.
  - Fecha de nacimiento y si la fecha es estimada.
- **Dar de Baja:**
  - Registro de baja con fecha y motivo.
- **Criterios de Validación:**
  - Rechazar identificación vacía o repetida en la misma granja.
  - La misma identificación en otra granja sí se acepta.
  - El animal dado de baja sigue siendo consultable, pero no admite nuevos eventos.
- **Reglas de Acceso / Base de Datos:**
  - Gestionan las fichas el **dueño** y el **trabajador** (según RF-02).
