# CLAUDE.md — SIRA (Sistema Integral de Registro de Ausencias)

Guía para agentes de IA que trabajan en este repositorio. Generada a partir de
la exploración del código en la rama `main`.

## Descripción
Sistema web ASP.NET Core 8 MVC para la gestión de inasistencias escolares del
**Instituto Agustín Rangel** y **SENA CAE Curumaní**. Permite registrar excusas
de inasistencia, gestionarlas (aprobar/rechazar con notificación al acudiente) y
administrar estudiantes, acudientes, administradores e instituciones educativas.

## Stack
- **Backend:** ASP.NET Core 8 MVC (`net8.0`), Entity Framework Core 8 (`8.0.10`)
- **DB:** SQLite — archivo `sira.db` (connection string `SiraDb`)
- **Frontend:** Bootstrap 5, Razor Views (`.cshtml`), JS vanilla, jQuery validation
- **Patrones:** SOLID, Repository, Arquitectura Onion
- **Autenticación:** Cookie Authentication + estado de roles en `Session`

---

## ⚠️ Reglas críticas para el agente

1. **NUNCA modificar la base de datos sin confirmación explícita del usuario.**
   Aplica en **cualquier modo** (plan, auto, ask). Incluye:
   - `INSERT`, `UPDATE`, `DELETE`, `ALTER`, `DROP`, `CREATE`
   - Migraciones EF Core (`dotnet ef migrations`, `dotnet ef database update`)
   - Scripts SQL manuales sobre `sira.db`
   - Cualquier cambio que afecte schema o datos

   Antes de tocar la BD: preguntar con `AskUserQuestion` (o texto directo) y
   esperar aprobación explícita.

2. **NUNCA realizar cambios de arquitectura sin confirmación explícita.**
   Aplica en **cualquier modo**. Incluye:
   - Crear una nueva capa (Application Services, Domain Services, UseCases)
   - Mover lógica entre capas (ej. extraer lógica de Controllers a Services)
   - Renombrar/reorganizar carpetas (`Controllers/`, `Repositories/`, etc.)
   - Cambiar el patrón de DI o el ciclo de vida (Scoped/Singleton/Transient)
   - Dependencias entre módulos que rompan el flujo Onion (interior → exterior)
   - Refactors que toquen más de un archivo de capa distinta

   Antes de un cambio arquitectónico: explicarlo, mostrar antes/después y pedir
   aprobación. Para cambios grandes, usar `/architecture` y crear un ADR primero.

3. **Mostrar cambios de código antes de aplicarlos** (bloque antes/después o
   diff) y esperar aprobación.

4. **Usar los agentes/plugins especializados** cuando aplique:
   `/architecture` (diseño/ADRs) · `/code-review` (PRs y diffs) ·
   `/debug` (debugging estructurado) · `/tech-debt` (deuda técnica).

---

## Comandos (build / run / EF)

```bash
# Restaurar dependencias
dotnet restore SIRA/SIRA.csproj

# Compilar
dotnet build SIRA/SIRA.csproj

# Ejecutar (desde la carpeta del proyecto, usa sira.db relativo)
dotnet run --project SIRA/SIRA.csproj

# EF Core (⚠️ requieren confirmación explícita — ver regla crítica #1)
dotnet ef migrations add <Nombre> --project SIRA/SIRA.csproj
dotnet ef database update         --project SIRA/SIRA.csproj
```

> No hay proyecto de pruebas en el repositorio.

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
│ │ │ │  Domain (Entities, Interfaces)  │ │ │ │  ← interior
│ │ │ └─────────────────────────────────┘ │ │ │
│ │ └─────────────────────────────────────┘ │ │
│ └─────────────────────────────────────────┘ │
└─────────────────────────────────────────────┘
```

**Regla de dependencias:** una capa interior NUNCA depende de una exterior.
Las dependencias fluyen siempre hacia adentro vía interfaces (DIP).

### Mapeo a la estructura real del proyecto

```
SIRA/
├── Models/                   # Domain — entidades de negocio (Excusa, Estudiante,
│                             #   Acudiente, Usuario, Administrador, TipoDocumento,
│                             #   EvidenciaExcusa)
├── Models/Entities/          # Domain — entidades (InstitucionEducativa, Auditoria)
├── ViewModels/               # Presentation — DTOs hacia/desde Views
├── Repositories/Interfaces/  # Domain — contratos (puertos)
├── Repositories/Implementations/  # Infrastructure — adaptadores EF Core
├── Services/                 # Infrastructure — IEmailService / EmailService (SMTP)
├── Configuration/            # Infrastructure — EmailSettings
├── Data/AppDbContext.cs      # Infrastructure — EF Core DbContext
├── Controllers/              # Presentation + Application — orquestación
├── Views/                    # Presentation — Razor (.cshtml)
└── wwwroot/                  # Presentation — assets estáticos + uploads/evidencias
```

> **Nota:** No existe una capa Application separada. La orquestación vive en los
> Controllers. Si un Controller crece demasiado, considerar extraer a un
> Application Service — pero ese cambio requiere confirmación (regla crítica #2).
>
> Las entidades están repartidas entre `Models/` y `Models/Entities/` (deuda
> técnica menor; unificar requiere aprobación).

---

## SOLID en el proyecto

- **SRP** — cada Controller/Service/Repository con una sola responsabilidad.
- **OCP** — nuevas funcionalidades vía nuevos repos/services, sin modificar
  interfaces existentes.
- **LSP** — toda implementación de una interfaz debe ser sustituible.
- **ISP** — interfaces específicas por entidad (`IAcudienteRepository`,
  `IExcusaRepository`, …) — sin interfaces "fat".
- **DIP** — Controllers/Services dependen de interfaces, inyectadas por
  constructor (todas registradas `Scoped` en `Program.cs`).

---

## Reglas operativas

- **Lógica de negocio:** en Controllers (por ahora) o futuros Application Services.
- **Validaciones de formulario:** DataAnnotations en ViewModels + JS en Views.
- **Acceso a datos:** SOLO en Repositories — nunca usar `AppDbContext` directo
  en Controllers.
- **Inyección de dependencias:** siempre por interfaces vía constructor.
- **try/catch** en Controllers — nunca propagar excepciones al usuario.
- **TempData** para mensajes éxito/error entre redirecciones.
- **Session** para roles y filtrado por institución; validar antes de ejecutar
  lógica protegida.
- **`ILogger`** para eventos de estado importantes (login, decisiones, etc.).

---

## Módulos activos

| Módulo | Controller | Notas |
|--------|------------|-------|
| Auth | `AuthController` | Login por alias/clave (cookie auth), roles en Session |
| Excusas | `ExcusasController` | Registro, evidencias, aprobación/rechazo + correo |
| Acudientes | `AcudientesController` | CRUD con institución educativa |
| Estudiantes | `EstudiantesController` | CRUD con institución educativa |
| Administradores | `DashboardController` | CRUD, super usuario, activar/inactivar |
| Instituciones | `InstitucionEducativaController` | CRUD (super usuario) |
| Consola SQL | `ConsolaSqlController` | Solo usuario root, con auditoría |
| Dashboard | `DashboardController` | Listado paginado de excusas |
| Home | `HomeController` | Páginas públicas (Index, Instituciones, Privacy) |

---

## Autenticación y roles

El login (`AuthController.Login`) valida alias/clave contra `usuario`, emite una
cookie de autenticación (`CookieAuthentication`, expiración 8h, sliding) y
guarda el estado de rol en `Session`:

| Session Key | Tipo | Descripción |
|-------------|------|-------------|
| `EsRoot` | int (0/1) | Acceso total + Consola SQL |
| `EsSuperUsuario` | int (0/1) | Gestión global de instituciones |
| `IdInstitucion` | int | Filtra datos por institución (0 si root/super) |
| `IdUsuario` | int | Id del usuario logueado |
| `Alias` | string | Alias del usuario logueado |

> ⚠️ **Seguridad (estado actual):** la clave se compara en **texto plano**
> (`usuario.Clave != vm.Clave`) y se almacena sin hash en la tabla `usuario`.
> Cualquier cambio de este comportamiento (hashing, etc.) toca la BD/datos →
> requiere confirmación (regla crítica #1).

---

## Base de datos

- **Motor:** SQLite · **Connection string:** `SiraDb = "Data Source=sira.db"`
  (ruta relativa al proyecto, definida en `appsettings.json`).
- **Tablas mapeadas** (vía `[Table("...")]` en entidades, `snake_case`):
  `usuario`, `administrador`, `acudiente`, `estudiante`, `excusa`,
  `evidencia_excusa`, `tipo_documento`, `institucion_educativa`, `auditoria`.
- **Convención:** clases/propiedades en `PascalCase`; tablas/columnas en
  `snake_case` mapeadas con `[Column("...")]`.
- **Relaciones** (configuradas en `AppDbContext.OnModelCreating`, todas FK
  opcionales `IsRequired(false)`):
  - `InstitucionEducativa` → `Administrador` (FK `id_administrador`)
  - `Estudiante` → `InstitucionEducativa` (FK `id_institucion_educativa`)
  - `Acudiente` → `InstitucionEducativa` (FK `id_institucion_educativa`)
- Archivos `.db` están marcados `CopyToPublishDirectory=Never` en el `.csproj`.

---

## Servicios externos

- **SMTP:** Gmail (`smtp.gmail.com:587`, `EnableSsl=true`). Credenciales en
  `appsettings.json` → sección `EmailSettings` (mapeada a `Configuration/EmailSettings.cs`).
- **EmailService** (`Services/EmailService.cs`, interfaz `IEmailService`):
  notificaciones de decisiones de excusas al acudiente, con cuerpo HTML + texto
  plano y headers anti-spam (Reply-To, List-Unsubscribe, X-Mailer).

> ⚠️ El password SMTP está versionado en `appsettings.json`. No exponerlo en
> logs ni en respuestas; considerar moverlo a secretos (requiere aprobación).

---

## Repositorio y ramas

- **GitHub:** https://github.com/ADSO-3412461/SIRA
- **Ramas:** `main`, `Developer`, `Testing`.
- Desarrollar en la rama indicada para la sesión; no hacer push a otra rama sin
  permiso explícito. No crear PRs salvo que el usuario lo pida.
