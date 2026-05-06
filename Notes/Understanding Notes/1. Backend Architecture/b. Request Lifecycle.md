# Request Lifecycle Understanding

# Files Involved

## API Layer

- `API/Program.cs`
- `API/Controllers/AuthController.cs`
- `API/Middlewares/CorrelationIdMiddleware.cs`
- `API/Middlewares/RequestLoggingMiddleware.cs`
- `API/Middlewares/ExceptionHandlingMiddleware.cs`

## Application Layer

- `Application/Features/Auth/Commands/Login/LoginCommand.cs`
- `Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`
- `Application/Common/Behaviors/ValidationBehavior.cs`

## Infrastructure Layer

- `Infrastructure/Repositories/UserRepository.cs`
- `Infrastructure/Data/AppDbContext.cs`

## Core Layer

- `Core/Entities/User.cs`

---

# Purpose

Understand exactly how an HTTP request moves through the backend system.

This is one of the most important backend engineering concepts because production debugging, observability, performance tuning, and system design all depend on understanding request execution flow.

---

# Full Login Request Flow

```text
HTTP Request
↓
Kestrel Web Server
↓
Middleware Pipeline
↓
Authentication Middleware
↓
Authorization Middleware
↓
Controller
↓
MediatR
↓
ValidationBehavior
↓
Handler
↓
Repository
↓
DbContext
↓
PostgreSQL
↓
Response returned upward
```

---

# Step-by-Step Execution

# Step 1 — Request Reaches ASP.NET Core

Client sends:

```http
POST /api/auth/login
```

with JSON body:

```json
{
  "email": "amina@test.com",
  "password": "password123"
}
```

ASP.NET Core binds JSON body into:

```csharp
LoginRequest request
```

inside:

```text
API/Controllers/AuthController.cs
```

---

# Step 2 — Request Enters Middleware Pipeline

Configured inside:

```text
API/Program.cs
```

Pipeline order:

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthentication();

app.UseAuthorization();
```

Middleware execution order is extremely important.

---

# Step 3 — CorrelationIdMiddleware Executes

Inside:

```text
API/Middlewares/CorrelationIdMiddleware.cs
```

Middleware checks:

```csharp
context.Request.Headers["X-Correlation-ID"]
```

If missing:

```csharp
Guid.NewGuid().ToString()
```

creates unique request identifier.

Stored in:

```csharp
context.Items["X-Correlation-ID"]
```

Then added to response headers.

Purpose:
- trace request across logs
- distributed tracing
- debugging production systems

---

# Step 4 — RequestLoggingMiddleware Executes

Inside:

```text
API/Middlewares/RequestLoggingMiddleware.cs
```

Middleware starts:

```csharp
var stopwatch = Stopwatch.StartNew();
```

Logs:

```csharp
Log.Information(...)
```

including:
- HTTP method
- request path
- correlation ID

Then:

```csharp
await _next(context);
```

passes control forward.

---

# Step 5 — Authentication Middleware

JWT token validated using configuration from:

```text
API/Extensions/AuthExtensions.cs
```

Token validation checks:
- issuer
- audience
- signature
- expiry

If invalid:
request blocked before reaching controller.

---

# Step 6 — Controller Executes

Inside:

```text
API/Controllers/AuthController.cs
```

Method:

```csharp
public async Task<IActionResult> Login(LoginRequest request)
```

Controller converts DTO into command:

```csharp
var command = new LoginCommand
{
    Email = request.Email,
    Password = request.Password
};
```

Important understanding:

Controller transforms transport-level request into application-level command.

---

# Step 7 — MediatR Executes

Controller calls:

```csharp
await _mediator.Send(command);
```

MediatR locates:

```text
LoginCommandHandler
```

inside:

```text
Application Layer
→ Features/Auth/Commands/Login/
```

---

# Step 8 — ValidationBehavior Executes

Inside:

```text
Application/Common/Behaviors/ValidationBehavior.cs
```

MediatR pipeline executes validators BEFORE handler.

Validator used:

```text
LoginCommandValidator
```

Validation rules:

```csharp
RuleFor(x => x.Email)
RuleFor(x => x.Password)
```

If validation fails:

```csharp
throw new ValidationException(...)
```

Handler never executes.

---

# Step 9 — LoginCommandHandler Executes

Inside:

```text
Application/Features/Auth/Commands/Login/LoginCommandHandler.cs
```

Handler responsibilities:
- fetch user
- verify password
- generate JWT
- return response

---

# Step 10 — Repository Access

Handler calls:

```csharp
_userRepository.GetUserByEmailAsync(
    request.Email,
    cancellationToken
);
```

Execution moves into:

```text
Infrastructure/Repositories/UserRepository.cs
```

---

# Step 11 — EF Core Executes Query

Repository uses:

```csharp
_context.Users.FirstOrDefaultAsync(...)
```

inside:

```text
Infrastructure/Data/AppDbContext.cs
```

EF Core translates LINQ into SQL query.

Example SQL behavior:

```sql
SELECT * FROM Users
WHERE Email = 'amina@test.com'
LIMIT 1
```

Returned row mapped into:

```csharp
User
```

entity from:

```text
Core/Entities/User.cs
```

---

# Step 12 — Password Verification

Inside handler:

```csharp
BCrypt.Net.BCrypt.Verify(
    request.Password,
    user.PasswordHash
);
```

Passwords are NOT decrypted.

BCrypt hashes incoming password again and compares hashes.

---

# Step 13 — JWT Token Generation

Handler calls:

```csharp
_jwtTokenService.GenerateToken(user)
```

Execution moves into:

```text
Infrastructure/Auth/JwtTokenService.cs
```

JWT created using:
- claims
- secret key
- signing credentials
- issuer
- audience
- expiry

---

# Step 14 — Response Returned

Handler returns:

```csharp
new LoginResponse
{
    Token = ...,
    Email = ...
}
```

Controller wraps response:

```csharp
ApiResponse<LoginResponse>.SuccessResponse(...)
```

Final HTTP response returned to client.

---

# Important Architectural Understanding

Each layer owns a specific responsibility:

| Layer          | Responsibility               |
|----------------|------------------------------
| API            | HTTP handling                |
| Middleware     | cross-cutting concerns       |
| Application    | workflows/orchestration      |
| Infrastructure | implementations              |
| Core           | entities/domain              |
| Database       | persistence                  |