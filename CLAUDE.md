# Claude Code Directives

> **MANDATORY:** Before writing, refactoring, or planning any code in this repo, read `./PROJECT_KNOWLEDGE.md` first. This file adds Claude-specific operating rules and points to the concrete code patterns below — use them as templates instead of inventing new shapes.

## Project

Sulozeqi-BackEnd — ASP.NET Core Web API (.NET), layered architecture, PostgreSQL via EF Core (Npgsql), JWT auth via HttpOnly cookie.

## Operating rules

1. Read `PROJECT_KNOWLEDGE.md` before implementing a feature or fix; keep new code consistent with its rules.
2. Match the concrete patterns in this file exactly (constructor style, response wrapping, error handling) rather than idiomatic-but-different .NET patterns from training data.
3. All code, comments, and identifiers in English.
4. No comments unless explaining a non-obvious WHY — this codebase currently has none; don't introduce doc-comment blocks.
5. Don't add tests, DTO validation frameworks, or abstractions the task doesn't need — this project has no test suite today; don't claim one exists or assume 80% coverage gates apply until the user adds a test project.

## Layers (in call order)

```
Controller/  → HTTP, [FromForm]/[FromBody] binding, no business logic, no DbContext
Services/    → business logic, owns AppDbContext, returns Response DTOs
Models/      → EF Core entities (AppDbContext, CommonData base)
Requests/    → inbound DTOs
Responses/   → outbound DTOs, always wrapped in BaseResponse<T>
```

Cross-cutting: `Filter/` (AutoWrapperFilter, ModelValidationFilter), `ExceptionMiddleware/`, `Middleware/`, `Extensions/`.

## Concrete patterns to copy

**Primary-constructor DI** — every controller and service takes dependencies via a primary constructor, not fields + injected constructor body:
```csharp
public class ProjectsController(ProjectService projectService) : BaseApiController
public class ProjectService(AppDbContext context, IWebHostEnvironment environment) : BaseService<Project>(context)
```

**Controllers** inherit `BaseApiController` (`[ApiController]`, `Route("api/[controller]")`), stay thin, delegate everything to a service, return `Ok(...)`/`BadRequest(...)` directly — never build a `BaseResponse<T>` by hand in a controller. See [Controller/ProjectsController.cs](Controller/ProjectsController.cs).

**Services** inherit `BaseService<T> where T : CommonData` ([Services/BaseService.cs](Services/BaseService.cs)) for `GetAllAsync`/`GetByIdAsync`/`DeleteAsync`, and override only what differs. Multi-step writes (create/update touching related entities) wrap in an EF Core transaction with try/rollback on catch — see `CreateProjectAsync`/`UpdateProjectAsync` in [Services/ProjectService.cs](Services/ProjectService.cs).

**Response wrapping is automatic** — `AutoWrapperFilter` ([Filter/AutoWrapperFilter.cs](Filter/AutoWrapperFilter.cs)) wraps every controller `ObjectResult`/`StatusCodeResult` in `BaseResponse<T>` globally. Do not manually construct `BaseResponse<T>` in controllers or services; just return the raw DTO or call `Ok()`/`BadRequest()`.

**Errors are exceptions, not manual status codes.** Throw `NotFoundException`/`BadRequestException` ([ExceptionMiddleware/Exceptions.cs](ExceptionMiddleware/Exceptions.cs)) from services; `ExceptionMiddleware` catches them globally and maps to the right HTTP status + `BaseResponse<object>` JSON. Don't catch-and-return error objects manually in controllers.

**Entities** inherit `CommonData` ([Models/CommonModel.cs](Models/CommonModel.cs)): `Id`, `DateTimeCreated`, `DateTimeUpdated`, `RowVersion` (optimistic concurrency) come for free — don't redeclare them.

**Multipart create/update with files**: JSON payload arrives as a `[FromForm] string` field, deserialized manually with `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`, files as a separate `[FromForm] List<IFormFile>` and attached onto the DTO afterward. See `CreateNewProject` in [Controller/ProjectsController.cs](Controller/ProjectsController.cs). File save/delete helpers live as private methods on the owning service (`SaveImageFileAsync`/`DeleteImageFile`), writing under `wwwroot/uploads/<entity>/`.

**Auth**: JWT issued by `AuthenticationService`, delivered via HttpOnly cookie (`X-Access-Token`) using `HttpContext.AppendAuthCookie` / `DeleteAuthCookie` extensions ([Extensions/Extensions.cs](Extensions/Extensions.cs)), read back via `JwtBearerEvents.OnMessageReceived` in [Program.cs](Program.cs). Admin credentials come from config, not a Users table. Protect actions with `[Authorize]`.

**DI registration** happens in [Program.cs](Program.cs): scoped for request-lifetime services (`ProjectService`, `AuthenticationService`, `ContactInquiryService`), singleton + hosted service for `VisitorCounterService`. Register new services the same way — never `new` one up in a controller.

## Verification

- Build before considering a change done: `dotnet build`
- There is no test project yet — don't claim tests pass or invent coverage numbers.
