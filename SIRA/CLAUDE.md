# SIRA - Sistema Integral de Registro de Ausencias

## Descripción
Sistema web ASP.NET Core 8 MVC para la gestión de inasistencias escolares del
Instituto Agustín Rangel y SENA CAE Curumaní.

## Stack
- **Backend:** ASP.NET Core 8 MVC, Entity Framework Core 8
- **DB:** SQLite (archivo `sira.db`)
- **Frontend:** Bootstrap 5, Razor Views (.cshtml), JS vanilla
- **Patrones:** SOLID, Repository, Arquitectura Onion

---

## ⚠️ Reglas críticas para el agente

1. **NUNCA modificar la base de datos sin confirmación explícita del usuario.**
   Esto aplica en **CUALQUIER modo** (plan, auto, ask, etc.). Incluye:
   - `INSERT`, `UPDATE`, `DELETE`, `ALTER`, `DROP`, `CREATE`
   - Migraciones de EF Core (`dotnet ef migrations`, `dotnet ef database update`)
   - Scripts SQL manuales sobre `sira.db`
   - Cualquier cambio que afecte schema o datos

   **Antes de cualquier cambio en BD:** preguntar al usuario con `AskUserQuestion`
   o texto directo y esperar aprobación explícita.

2. **NUNCA realizar cambios de arquitectura sin confirmación explícita del usuario.**
   Esto aplica en **CUALQUIER modo**. Incluye:
   - Crear una nueva capa (ej. Application Services, Domain Services, UseCases)
   - Mover lógica entre capas (ej. extraer lógica de Controllers a Services)
   - Renombrar/reorganizar carpetas (`Controllers/`, `Repositories/`, etc.)
   - Cambiar el patrón de inyección de dependencias o el ciclo de vida (Scoped/Singleton/Transient)
   - Agregar nuevos `using`/dependencias entre módulos que rompan el flujo Onion (interior → exterior)
   - Refactors que afecten más de un archivo de capa distinta

   **Antes de cualquier cambio arquitectónico:** explicar el cambio,
   mostrar el "antes/después" y pedir aprobación explícita. Si es un cambio
   grande, usar `/architecture` para crear un ADR primero.

3. **Mostrar cambios de código antes de aplicarlos.** Presentar bloque
   "antes/después" o diff y esperar aprobación.

4. **Usar los agentes/plugins especializados** cuando aplique:
   - `/architecture` — decisiones de diseño y ADRs
   - `/code-review` — revisión de PRs y diffs
   - `/debug` — debugging estructurado
   - `/tech-debt` — auditoría de deuda técnica

---

## Arquitectura Onion (capas concéntricas)

```
┌─────────────────────────────────────────────┐
│  Presentation (Controllers, Views, JS)      │  ← exterior
│ ┌─────────────────────────────────────────┐ │
│ │  Infrastructure (Repos impl., Services, │ │
│ │  AppDbContext, EmailService, SMTP)      │ │
│ │ ┌─────────────────────────────────────┐ │ │
│ │ │  Application (orquestación, UCs)    │ │ │
│ │ │ ┌─────────────────────────────────┐ │ │ │
│ │ │ │  Domain (Entities, Interfaces,  │ │ │ │
│ │ │ │  reglas de negocio puras)       │ │ │ │  ← interior
│ │ │ └─────────────────────────────────┘ │ │ │
│ │ └─────────────────────────────────────┘ │ │
│ └─────────────────────────────────────────┘ │
└─────────────────────────────────────────────┘
```

**Regla de dependencias:** una capa interior NUNCA depende de una exterior.
Las dependencias fluyen siempre hacia adentro vía interfaces (DIP).

### Mapeo a la estructura actual del proyecto

```
SIRA/
├── Models/Entities/          # Domain — entidades de negocio
├── Models/                   # Domain — modelos compartidos
├── ViewModels/               # Presentation — DTOs hacia/desde Views
├── Repositories/Interfaces/  # Domain — contratos (puertos)
├── Repositories/Implementations/  # Infrastructure — adaptadores EF Core
├── Services/                 # Infrastructure — EmailService, SMTP
├── Configuration/            # Infrastructure — settings (EmailSettings)
├── Data/AppDbContext.cs      # Infrastructure — EF Core DbContext
├── Controllers/              # Presentation + Application — orquestación
├── Views/                    # Presentation — Razor
└── wwwroot/                  # Presentation — assets estáticos
```

> **Nota:** El proyecto no tiene capa Application separada todavía. La
> orquestación vive en Controllers. Si un Controller crece demasiado, extraer
> a una clase Application Service — pero ese cambio requiere confirmación
> explícita del usuario (ver regla crítica #2).

---

## SOLID en el proyecto

- **SRP** — cada Controller/Service/Repository tiene una sola responsabilidad
- **OCP** — agregar nuevas funcionalidades vía nuevos repos/services, no
  modificando interfaces existentes
- **LSP** — toda implementación de una interfaz debe ser sustituible
- **ISP** — interfaces específicas por entidad (`IAcudienteRepository`,
  `IExcusaRepository`) — no interfaces "fat"
- **DIP** — Controllers/Services dependen de interfaces, nunca de
  implementaciones concretas. Inyección por constructor.

---

## Reglas operativas

- **Lógica de negocio**: en Controllers (por ahora) o Application Services
- **Validaciones de formulario**: DataAnnotations en ViewModels + JS en Views
- **Acceso a datos**: SOLO en Repositories (nunca DbContext directo en
  Controllers)
- **Inyección de dependencias**: siempre por interfaces vía constructor
- **try/catch** obligatorio en Controllers, nunca lanzar excepciones al usuario
- **TempData** para mensajes éxito/error entre redirecciones
- **Session** para roles y filtrado por institución

---

## Módulos activos

| Módulo | Controller | Notas |
|--------|------------|-------|
| Auth | `AuthController` | Login por alias/clave, roles en Session |
| Excusas | `ExcusasController` | Registro, aprobación/rechazo, evidencias, envío de correo |
| Acudientes | `AcudientesController` | CRUD con institución educativa |
| Estudiantes | `EstudiantesController` | CRUD con institución educativa |
| Administradores | `DashboardController` | CRUD, super usuario, activar/inactivar |
| Instituciones Educativas | `InstitucionEducativaController` | CRUD solo super usuario |
| Consola SQL | `ConsolaSqlController` | Solo usuario root, con auditoría |
| Dashboard | `DashboardController` | Listado paginado de excusas |

---

## Roles del sistema (en Session)

| Session Key | Descripción |
|-------------|-------------|
| `EsRoot == 1` | Acceso total + Consola SQL |
| `EsSuperUsuario == 1` | Gestión global de instituciones |
| `IdInstitucion` | Filtra datos por institución |

---

## Base de datos

- **Motor**: SQLite
- **Archivo**: `sira.db` (ruta relativa al proyecto)
- **Tablas**: `usuario`, `administrador`, `acudiente`, `estudiante`, `docente`,
  `excusa`, `evidencia_excusa`, `tipo_documento`, `institucion_educativa`,
  `auditoria`
- **Convención**: tablas en `snake_case`, columnas mapeadas via
  `[Column("...")]` en entidades

---

## Convenciones de código

- Clases y propiedades: `PascalCase`
- Tablas y columnas SQLite: `snake_case`
- Validar `Session` antes de ejecutar lógica protegida
- Siempre `try/catch` en Controllers — nunca lanzar al usuario
- `TempData` para mensajes entre redirecciones
- Cada cambio de estado importante debe loguearse con `ILogger`

---

## Servicios externos

- **SMTP**: Gmail (`smtp.gmail.com:587`, EnableSsl=true). Credenciales en
  `appsettings.json` → `EmailSettings`
- **EmailService**: envío de notificaciones de excusas (HTML + texto plano,
  headers anti-spam Reply-To, List-Unsubscribe, X-Mailer)
