# Exception Handling Understanding

# Files Involved

## API Layer

- `API/Middlewares/ExceptionHandlingMiddleware.cs`
- `API/Program.cs`

## Application Layer

- `Application/Common/Exceptions/BadRequestException.cs`
- `Application/Common/Exceptions/DuplicateEmailException.cs`
- `Application/Common/Exceptions/NotFoundException.cs`
- `Application/Common/Exceptions/UnauthorizedException.cs`

## Application Features

- `Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`
- `Application/Features/Users/Commands/CreateUser/CreateUserCommandHandler.cs`
- `Application/Features/AI/Queries/GetAIStatus/GetAIStatusQueryHandler.cs`

---

# Layer

```text
Cross-cutting Error Handling Infrastructure
```

Exception handling spans:
- middleware
- application workflows
- API responses

because failures can occur anywhere in the execution pipeline.

---

# Purpose

Exception handling provides:

```text
Centralized Failure Management
```

Purpose:
- catch unhandled exceptions
- standardize error responses
- prevent application crashes
- improve debugging
- improve observability

---

# Why Centralized Exception Handling Is Important

Without centralized middleware:

```text
Every controller/handler
would need repetitive try-catch blocks.
```

Problems:
- duplicated logic
- inconsistent responses
- difficult maintenance
- poor scalability

Centralized middleware solves this cleanly.

---

# Current Exception Flow

```text
Request
↓
Controller
↓
Handler
↓
Repository/Service
↓
Exception thrown
↓
ExceptionHandlingMiddleware catches exception
↓
Standardized HTTP response returned
```

---

# Middleware Registration Order

## File

```text
API/Program.cs
```

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

Placed FIRST in pipeline.

Very important.

Why?

Because it must catch:
- controller exceptions
- middleware exceptions
- handler exceptions
- repository exceptions

from entire downstream pipeline.

---

# ExceptionHandlingMiddleware Understanding

# File

```text
API/Middlewares/ExceptionHandlingMiddleware.cs
```

Purpose:
- global exception interception
- centralized error response handling

---

# Middleware Execution Pattern

Inside middleware:

```csharp
try
{
    await _next(context);
}
catch (Exception ex)
{
    ...
}
```

Important understanding:

```text
_next(context)
```

executes ALL downstream middleware/controllers.

Any downstream exception bubbles upward into this middleware.

---

# Exception Bubbling Understanding

Example:

```text
LoginCommandHandler
↓
throw UnauthorizedException
↓
MediatR
↓
Controller
↓
Middleware Pipeline
↓
ExceptionHandlingMiddleware catches it
```

This is called:

```text
Exception Propagation/Bubbling
```

---

# Custom Exceptions Understanding

Current backend defines custom application exceptions.

# Files

```text
Application/Common/Exceptions/
```

---

# BadRequestException

```csharp
BadRequestException
```

Used for:
- invalid business requests
- invalid application state

Usually maps to:

```http
400 Bad Request
```

---

# DuplicateEmailException

```csharp
DuplicateEmailException
```

Used when:
- email already exists

Example:

```csharp
throw new DuplicateEmailException(
    request.Email
);
```

Usually maps to:

```http
409 Conflict
```

or:

```http
400 Bad Request
```

depending on middleware implementation.

---

# NotFoundException

```csharp
NotFoundException
```

Used when:
- entity/resource missing

Example:

```csharp
throw new NotFoundException(
    "Job not found"
);
```

Usually maps to:

```http
404 Not Found
```

---

# UnauthorizedException

```csharp
UnauthorizedException
```

Used when:
- authentication fails
- invalid credentials

Example:

```csharp
throw new UnauthorizedException(
    "Invalid email or password"
);
```

Usually maps to:

```http
401 Unauthorized
```

---

# Login Failure Flow

# File

```text
Application/Features/Auth/Commands/Login/LoginCommandHandler.cs
```

---

# Step 1 — User Not Found

```csharp
if (user is null)
{
    throw new UnauthorizedException(...);
}
```

---

# Step 2 — Password Invalid

```csharp
if (!isPasswordValid)
{
    throw new UnauthorizedException(...);
}
```

---

# Step 3 — Exception Bubbles Up

Exception propagates upward through:
- handler
- MediatR
- controller
- middleware pipeline

until:

```text
ExceptionHandlingMiddleware
```

catches it.

---

# AI Job Failure Flow

# File

```text
Application/Features/AI/Queries/GetAIStatus/GetAIStatusQueryHandler.cs
```

Example:

```csharp
if (job is null)
{
    throw new NotFoundException(
        "Job not found"
    );
}
```

This eventually becomes:

```http
404 Not Found
```

response.

---

# Why Application Layer Throws Exceptions

Very important architecture understanding.

Application layer should define:
- business/application failures

Application layer should NOT:
- create HTTP responses directly

Why?

Because Application layer must remain:
- HTTP-independent
- framework-independent

---

# Middleware Responsibility

Middleware translates:

```text
Exceptions
↓
HTTP Responses
```

This separation is extremely important.

---

# Standardized Error Responses

Purpose:
- consistent API behavior
- predictable frontend integration
- easier debugging

Example standardized response:

```json
{
  "success": false,
  "message": "Invalid email or password"
}
```

---

# Logging Exceptions

Exception middleware usually logs:
- exception type
- message
- stack trace
- correlation ID

This is critical for production debugging.

---

# Exception vs Validation Failures

Important distinction:

| Type | Purpose |
|---|---|
| Validation | invalid input |
| Exceptions | runtime/business failures |

Validation occurs BEFORE handlers.

Exceptions occur DURING execution.

---

# ValidationBehavior Integration

Example:

```csharp
throw new ValidationException(...)
```

inside:

```text
ValidationBehavior
```

These exceptions also bubble into global middleware.

---

# Important Variables Understanding

| Variable | Purpose |
|---|---|
| `ex` | caught exception |
| `_next` | next middleware |
| `request` | incoming workflow request |
| `user` | fetched entity |
| `job` | AI job entity |

---

# Production Relevance

Centralized exception handling critical in:
- enterprise APIs
- distributed systems
- microservices
- AI platforms
- SaaS applications

because production systems must:
- fail safely
- expose clean responses
- avoid crashes
- support observability

---

# Most Important Architectural Understanding

Exceptions should:
- propagate upward naturally
- be centralized at middleware boundary
- remain separated from HTTP concerns

Current backend correctly separates:

| Layer       | Responsibility                         |
|-------------|----------------------------------------|
| Application | define failures                        |
| Middleware  | convert failures into HTTP responses   |
| Logging     | observe failures                       |

This creates:
- clean architecture
- maintainable error handling
- scalable observability
- production-grade API behavior