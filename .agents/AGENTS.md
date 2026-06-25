# Glowtics Architectural Rules and Decisions

This file documents the core architectural rules and decisions made for the Glowtics project. **All contributors must strictly adhere to these rules when modifying or adding code to the project.**

### 1. High Cohesion CQRS Files
- **Decision:** The Command/Query, its Handler, and its specific Response model must all be defined within the **same C# file**. 
- **Rule:** Inside the file, the structural order must strictly be: `Request` (top) -> `Response` -> `[Validator]` -> `Handler` (bottom).
- **Reason:** Following standard MediatR and Vertical Slice CQRS patterns, keeping the Request, Response, and Handler together maximizes cohesion. It drastically improves discoverability and reduces cognitive load by eliminating the need to jump across different folders (e.g., `Commands/` vs `Responses/`) when working on a single feature, because things that change together should live together. The strict top-to-bottom ordering creates a clear "Input -> Output -> Implementation" mental model.

### 2. Strict CQRS with Orchestrators
- **Decision:** A MediatR Command must strictly modify the state of a **single entity**. If a workflow requires modifying multiple distinct entities across different domains (e.g., creating a User Identity, assigning a Role, and creating a Retailer Profile), it must be handled by an **Orchestrator**.
- **Reason:** This prevents the dangerous anti-pattern of MediatR Handlers calling other MediatR Handlers, which creates tight coupling, hidden dependencies, and spaghetti code.

### 3. Security & Exception Handling
- **Exception-Driven Flow:** Do not return complex failure objects from BLL Handlers. Throw pure domain exceptions (e.g., `DomainException`, `InvalidCredentialsException`).
- **Unified Base Exception:** All custom business exceptions MUST inherit from an abstract `GlowticsException` (never directly from `System.Exception`) so middleware can safely catch them without intercepting catastrophic system crashes.
- **No API Exceptions in BLL:** The BLL must never throw HTTP-centric exceptions like `BadRequestException` or `NotFoundException`.
- **Auth-Specific Exceptions:** Throw specific exceptions for authentication failures (e.g., `InvalidCredentialsException`) rather than generic domain exceptions, allowing the middleware to map them to `401 Unauthorized`.
- **Preserve InnerExceptions:** Exceptions that touch infrastructure (e.g., database errors) MUST preserve the original system exception as an `InnerException` to avoid losing the stack trace.
- **Global Middleware:** BLL domain exceptions bubble up to the API layer, where `ExceptionHandlingMiddleware` transforms them into standardized `ApiResponse.Failure` JSON envelopes mapped to the correct HTTP status codes.
- **Reason:** The BLL should have absolutely zero knowledge of APIs, HTTP contexts, or status codes. Centralizing exception logic in middleware ensures consistent API responses and prevents BLL leakage into the presentation layer.

### 4. No Magic Strings for Security
- **Decision:** Authorization roles, claims, and policies must not be hardcoded as magic strings (e.g., `[Authorize(Roles = "Retailer")]`). Instead, they must be defined as static constants **within the API layer** (e.g., `Glowtics.Api.Constants.Roles` or `Policies`), entirely separate from the BLL.
- **Reason:** Centralizing security definitions prevents typos. Defining them in the API layer rather than the BLL ensures the API can manage its own complex authorization policies without leaking web-specific security structures into the business domain.

### 5. Standardized API Responses
- **Decision:** Every API endpoint must return an `ApiResponse` or `ApiResponse<T>` wrapper object.
- **Reason:** This ensures front-end clients always receive a predictable, consistent JSON structure containing `IsSuccess`, `Message`, `Errors`, and `Data` for every single request, simplifying client-side error handling.

### 6. Secure API Key Storage
- **Decision:** API Keys must be hashed before being stored in the database. The raw key is only returned to the user exactly once upon generation.
- **Reason:** Following industry-standard security practices, this ensures that if the database is compromised, attackers cannot steal plain-text API keys to impersonate retailers.

### 7. No Primitive Returns
- **Decision:** A MediatR Query or Command that returns data MUST NEVER return a primitive type directly (e.g., `Guid` or `string`). It MUST always return a dedicated Response object even if it only contains one property on Day 1.
- **Reason:** This guarantees future-proof CQRS extensibility. If business requirements change and more fields must be returned, the Response object can be expanded without breaking the command handler's signature or API contract.

### 8. No Custom Repositories or Unit of Work Wrappers
- **Decision:** Do not create custom Generic Repository (`IRepository<T>`) or Unit of Work (`IUnitOfWork`) abstractions around Entity Framework Core. Inject the `GlowticsDbContext` directly into MediatR handlers.
- **Reason:** EF Core's `DbContext` is already a fully featured Unit of Work, and its `DbSet<T>` is already a generic repository. There are no intentions to change the database or the ORM in the future, therefore an extra layer of abstraction is not needed.
- <span style="color:red">**CRITICAL RULE:</span>  To enforce Decision #2, the injected DbContext MUST be used to update only ONE entity (DbSet) within the handler.**


### 9. Modern C# Null Checking (`?? throw`)
- **Decision:** Use the null-coalescing throw expression directly on EF Core queries instead of traditional multi-line `if (null)` blocks.
  - *Example:* `var entity = await db.FirstOrDefaultAsync() ?? throw new EntityNotFoundException();`

