SYSTEM INSTRUCTION FOR ALL AI ASSISTANTS: Before implementing, refactoring, or planning ANY feature or fix in this repository, you MUST read this file first. All code generated MUST strictly adhere to the patterns and constraints listed below. Claude Code additionally reads `CLAUDE.md`, which points back to the concrete code patterns below.

# PROJECT_KNOWLEDGE.md

## 1. Executive Summary & Architecture Overview

This document outlines the architecture, coding standards, and best practices for the **Sulozeqi-BackEnd** project.

The project is a **.NET Core ASP.NET Web API** that follows a standard layered architecture. This design separates concerns, making the application more maintainable, scalable, and testable. The primary layers are:

- **Presentation Layer (`Controller`):** Handles HTTP requests, authentication, and data validation. It is the entry point of the application.
- **Service Layer (`Services`):** Contains the core business logic. It orchestrates data from different sources (like the database) and performs the main operations of the application.
- **Data Access Layer (`Models`, `Migrations`):** Manages data persistence using Entity Framework Core. It includes database entities and migration files.
- **Cross-Cutting Concerns (`Extensions`, `ExceptionMiddleware`, `Filter`):** Includes functionalities that are used across different layers, such as dependency injection, exception handling, and request filtering.

## 2. Tech Stack

| Component           | Technology                               | Justification                                                                                             |
| ------------------- | ---------------------------------------- | --------------------------------------------------------------------------------------------------------- |
| **Web Framework**   | ASP.NET Core                             | A high-performance, cross-platform framework for building modern, cloud-based, Internet-connected applications. |
| **ORM**             | Entity Framework Core                    | The standard ORM for .NET Core, simplifying database interactions and enabling LINQ queries.               |
| **Database**        | PostgreSQL (via Npgsql)                  | A powerful, open-source object-relational database system.                                                |
| **Authentication**  | JWT (JSON Web Tokens), delivered via HttpOnly cookie | A standard, secure way to handle authentication for APIs.                                     |
| **Testing**         | None yet                                 | No test project exists in this repo today; add xUnit/NUnit when tests are introduced.                     |
| **API Documentation** | Swagger (Swashbuckle)                    | Automatically generates API documentation from the code, making it easy to explore and test the API.        |

## 3. Mandatory Coding Rules & Constraints

To maintain code quality and consistency, all developers and AI assistants must adhere to the following rules:

- **File-Scoped Namespaces:** All new C# files must use file-scoped namespaces to reduce nesting and improve readability (e.g., `namespace Sulozeqi_BackEnd.Services;`).
- **Naming Conventions:**
    - Classes, methods, and properties: `PascalCase` (e.g., `MyClass`, `GetValue`).
    - Method parameters and local variables: `camelCase` (e.g., `myParameter`).
  -   Interfaces: `IPascalCase` (e.g., `IMyService`).
- **Asynchronous Programming:**
  -   Use `async/await` for all I/O-bound operations (database access, HTTP calls).
    - All async methods must end with the `Async` suffix (e.g., `GetValueAsync`).
- **Dependency Injection (DI):**
    - All services and repositories must be registered in `Program.cs` and injected via **primary constructors** (e.g. `public class ProjectService(AppDbContext context) : BaseService<Project>(context)`), not constructor bodies.
    - Do not use `new` to create instances of services or repositories.
- **API Design:**
  -   Use `Requests` and `Responses` DTOs (Data Transfer Objects) for all controller actions to decouple the API contract from the database models.
  -   Use HTTP verbs correctly (`GET` for retrieval, `POST` for creation, `PUT` for updates, `DELETE` for removal).
- **Forbidden Practices:**
    - Do not write business logic in controllers. Controllers should only delegate to services.
    - Do not access `DbContext` directly from controllers. All database operations must go through the service layer.

## 4. Standard Solutions Repository

This section provides standard solutions for common tasks.

### API Requests & Responses

- **Request Handling:** A request flows from the `Controller` to the `Service`. The controller is responsible for model validation and returning the appropriate HTTP status code (`Ok(...)`, `BadRequest(...)`, etc.) — never throw together an error response by hand.
- **Response Wrapping:** Every controller response is auto-wrapped in `BaseResponse<T>` (`Success`, `Message`, `Data`) by the global `AutoWrapperFilter`. Controllers/services just return the raw DTO or call `Ok()`.
- **Error Handling:** Services throw `NotFoundException` / `BadRequestException` (`ExceptionMiddleware/Exceptions.cs`); the global `ExceptionMiddleware` catches them and maps to the right HTTP status plus a standardized `BaseResponse<object>` error body.
- **Authentication:** JWT-based authentication is used, but the token is delivered via an HttpOnly cookie (`X-Access-Token`), not the `Authorization` header — see `JwtBearerEvents.OnMessageReceived` in `Program.cs` and the `AppendAuthCookie`/`DeleteAuthCookie` extensions. An `[Authorize]` attribute should be used on controllers or actions that require authentication. Admin credentials come from configuration, not a database table.
- **File Uploads:** Multipart create/update endpoints accept the JSON payload as a `[FromForm] string` (deserialized manually) plus a separate `[FromForm] List<IFormFile>`; files are saved under `wwwroot/uploads/<entity>/` by private helpers on the owning service.

### Database Access

- Database access is managed by Entity Framework Core.
- The `DbContext` is registered as a scoped service.
- All database queries should be asynchronous and use LINQ.

## 5. File Structure Blueprint

```
Sulozeqi-BackEnd/
├── Controller/
│   └── *.cs             # API controllers
├── Services/
│   └── *.cs             # Business logic
├── Models/
│   └── *.cs             # Database entities
├── Requests/
│   └── *.cs             # API request DTOs
├── Responses/
│   └── *.cs             # API response DTOs
├── Migrations/
│   └── *.cs             # EF Core migrations
├── Extensions/
│   └── *.cs             # Service registration and other extensions
├── ExceptionMiddleware/
│   └── *.cs             # Global exception handling
├── Properties/
│   └── launchSettings.json
├── wwwroot/
├── Program.cs           # Application entry point and DI configuration
├── appsettings.json
└── Sulozeqi-BackEnd.csproj
```

## 6. Verification Rules

- **Build:** `dotnet build` must succeed before a change is considered done.
- **Tests:** There is no test project in this repo yet. Do not claim tests pass, invent coverage numbers, or assume a coverage gate — if the user asks for tests, propose adding an xUnit/NUnit project first.
- **Pull Requests:** All new code must be submitted via a pull request and reviewed by at least one other team member before being merged into the main branch.