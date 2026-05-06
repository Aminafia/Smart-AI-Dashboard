# CQRS + MediatR Understanding

# Files Involved

## API Layer

- `API/Controllers/AuthController.cs`

## Application Layer

- `Application/Features/Auth/Commands/Login/LoginCommand.cs`
- `Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`
- `Application/Common/Behaviors/ValidationBehavior.cs`
- `Application/Features/Users/Queries/GetUsers/GetUsersQuery.cs`
- `Application/Features/Users/Queries/GetUsers/GetUsersQueryHandler.cs`

---

# Layer

```text
Application Layer
```

CQRS and MediatR primarily belong to the Application layer because they orchestrate workflows and separate read/write responsibilities.

---

# Purpose

CQRS separates:

```text
Commands
```

from:

```text
Queries
```

Commands:
- modify state

Queries:
- fetch data

MediatR acts as an internal request routing system between:
- controllers
- handlers

---

# Core CQRS Principle

## Commands

Commands change application state.

Examples from current backend:

```text
LoginCommand
CreateUserCommand
GenerateAICommand
RegisterUserCommand
```

Commands usually:
- create
- update
- delete
- trigger workflows

---

## Queries

Queries fetch/read data.

Examples:

```text
GetUsersQuery
GetAIStatusQuery
```

Queries should NOT modify state.

---

# Why CQRS Is Important

Without CQRS:

```text
Controllers become large
Business logic becomes scattered
Architecture becomes tightly coupled
```

With CQRS:

```text
Controller
↓
Command/Query
↓
Handler
↓
Repository
```

Responsibilities become isolated and scalable.

---

# MediatR Understanding

MediatR acts like:

```text
internal message bus
```

Controllers send requests through MediatR instead of directly calling services.

---

# Login Command Flow

# Step 1 — Controller Receives Request

## File

```text
API/Controllers/AuthController.cs
```

Client sends:

```http
POST /api/auth/login
```

JSON body becomes:

```csharp
LoginRequest request
```

---

# Step 2 — Controller Creates Command

Inside:

```csharp
AuthController.Login(LoginRequest request)
```

controller creates:

```csharp
var command = new LoginCommand
{
    Email = request.Email,
    Password = request.Password
};
```

Important understanding:

Controller transforms transport DTO into application command.

---

# Step 3 — MediatR Receives Command

Controller calls:

```csharp
await _mediator.Send(command);
```

`_mediator`
is injected through Dependency Injection.

MediatR now becomes responsible for routing execution.

---

# Step 4 — ValidationBehavior Executes

Before handler executes:

```text
ValidationBehavior<TRequest, TResponse>
```

runs automatically.

## File

```text
Application/Common/Behaviors/ValidationBehavior.cs
```

---

# Validator Resolution

DI injects validators:

```csharp
IEnumerable<IValidator<TRequest>>
```

For:

```text
LoginCommand
```

MediatR resolves:

```text
LoginCommandValidator
```

---

# Validation Execution

Validation executes:

```csharp
v.ValidateAsync(context, cancellationToken)
```

Rules:

```csharp
RuleFor(x => x.Email)
RuleFor(x => x.Password)
```

If validation fails:

```csharp
throw new ValidationException(failures)
```

Handler execution stops immediately.

---

# Step 5 — Handler Executes

MediatR routes command into:

```text
LoginCommandHandler
```

## File

```text
Application/Features/Auth/Commands/Login/LoginCommandHandler.cs
```

Handler responsibilities:
- fetch user
- verify password
- generate JWT
- return response

---

# Important Dependency Understanding

Handler depends on abstractions:

```csharp
private readonly IUserRepository _userRepository;

private readonly IJwtTokenService _jwtTokenService;
```

NOT concrete implementations.

This preserves Clean Architecture.

---

# Query Flow Understanding

Example:

```text
GetUsersQuery
```

## File

```text
Application/Features/Users/Queries/GetUsers/
```

Flow:

```text
Controller
↓
GetUsersQuery
↓
GetUsersQueryHandler
↓
Repository
↓
Database
↓
List<UserDto>
```

Queries fetch data only.

---

# DTO Understanding

Handlers usually return:
- DTOs
- response models

NOT entities directly.

Example:

```csharp
UserDto
LoginResponse
GenerateAIResponse
```

Purpose:
- protect internal domain models
- control API response shape
- decouple persistence from transport

---

# Important Variables Understanding

| Variable | Purpose |
|---|---|
| `_mediator` | routes requests |
| `request` | incoming command/query |
| `_validators` | FluentValidation validators |
| `_userRepository` | data access abstraction |
| `_jwtTokenService` | token generation abstraction |

---

# Why Handlers Are Important

Handlers centralize workflow logic.

Instead of:

```text
fat controllers
```

you now have:

```text
thin controllers
+
isolated workflows
```

This improves:
- maintainability
- scalability
- testability
- readability

---

# Production Relevance

CQRS heavily used in:
- enterprise APIs
- distributed systems
- microservices
- event-driven systems
- AI workflow systems

because it scales workflow complexity cleanly.

---

# Most Important Architectural Understanding

Controllers should NOT contain:
- business logic
- orchestration
- persistence logic

Controllers should:
- receive requests
- create commands/queries
- delegate to MediatR
- return responses

Application handlers own workflow orchestration.