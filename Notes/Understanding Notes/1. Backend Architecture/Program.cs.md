# Program.cs — Runtime + Pipeline Understanding

## Program.cs Role

```text
Composition Root of the application
```

Responsibilities:

* Register services
* Configure DI container
* Configure middleware pipeline
* Configure infrastructure
* Configure authentication
* Configure observability
* Build application runtime

---

# Application Layers

```text
API
↓
Application
↓
Core
↓
Infrastructure
```

Only API layer knows all layers.

`Program.cs` wires everything together.

---

# Startup Flow

```text
builder = Configure Services
app = Configure Request Pipeline
```

---

# WebApplication.CreateBuilder(args)

Creates:

* DI container
* Configuration system
* Logging system
* Routing system
* Kestrel web server
* Middleware builder

---

# Configuration System

```csharp
var configuration = builder.Configuration;
```

Reads:

* appsettings.json
* appsettings.Development.json
* environment variables
* secrets
* command-line args

---

# Connection String

```csharp
var connectionString =
    configuration.GetConnectionString("DefaultConnection");
```

Purpose:

* Centralized infrastructure configuration
* Avoid hardcoded DB values

---

# Fail Fast Principle

```csharp
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(...);
}
```

Purpose:

* Crash immediately on invalid startup configuration
* Prevent partial/broken application startup

---

# Serilog Setup

```csharp
Log.Logger = new LoggerConfiguration()
```

Purpose:

* Structured logging system

Features:

* Console logging
* File logging
* Context enrichment
* Centralized observability

---

# Structured Logging

Instead of:

```text
Console.WriteLine()
```

Use:

```json
{
  "Timestamp": "...",
  "RequestId": "...",
  "Message": "..."
}
```

Benefits:

* Tracing
* Monitoring
* Kibana/Grafana support
* Distributed observability

---

# Log Context Enrichment

```csharp
.Enrich.FromLogContext()
```

Enables:

* Correlation IDs
* Request metadata
* Shared request logging context

---

# Log Sinks

```csharp
.WriteTo.Console()
```

Used for:

* Docker logs
* Kubernetes logs
* local debugging

```csharp
.WriteTo.File(...)
```

Used for:

* persistent logs
* rolling log files

---

# UseSerilog()

```csharp
builder.Host.UseSerilog();
```

Replaces default ASP.NET logging pipeline.

---

# Cross-Cutting Concerns

Infrastructure concerns applied globally:

* Logging
* Authentication
* Authorization
* Caching
* Rate limiting
* Exception handling
* Observability

---

# AddControllers()

```csharp
builder.Services.AddControllers();
```

Registers:

* MVC pipeline
* routing
* model binding
* JSON serialization
* controller activation

---

# Dependency Injection Container

```csharp
builder.Services
```

Represents:

```text
IServiceCollection
```

Purpose:

* Dependency registry

Example:

```csharp
services.AddScoped<IUserRepository, UserRepository>();
```

Meaning:

```text
Inject UserRepository whenever IUserRepository is requested
```

---

# Swagger Configuration

Purpose:

* OpenAPI documentation
* API discoverability
* JWT testing support

---

# JWT Swagger Integration

```csharp
options.AddSecurityDefinition("Bearer", ...)
```

Adds:

* Authorization header support
* JWT token input inside Swagger UI

---

# Clean Architecture Observation

Program.cs contains:

* configuration
* orchestration
* infrastructure wiring

Program.cs does NOT contain:

* business logic
* DB queries
* application rules

---

# Middleware Pipeline Runtime Model

Middleware execution is nested.

Actual execution:

```text
ExceptionMiddleware
{
    CorrelationMiddleware
    {
        LoggingMiddleware
        {
            Authentication
            {
                Authorization
                {
                    Controller
                    {
                        MediatR
                        {
                            Handler
                            {
                                Repository
                                {
                                    DbContext
                                    {
                                        PostgreSQL
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
```

---

# _next(context)

```csharp
await _next(context);
```

Meaning:

```text
Continue pipeline execution
```

Without `_next()`:
pipeline stops.

---

# ExceptionHandlingMiddleware

Purpose:

* Global exception handling

Behavior:

```csharp
try
{
    await _next(context);
}
catch(Exception ex)
{
}
```

Wraps entire downstream pipeline.

---

# CorrelationIdMiddleware

Purpose:

* Request tracing

Stores request ID in:

```csharp
HttpContext.Items
```

Accessible by:

* middleware
* controllers
* logging
* services

---

# HttpContext

Represents:

* request state container

Contains:

* headers
* user claims
* response
* route data
* request-scoped storage

---

# RequestLoggingMiddleware

Logs:

* incoming request
* outgoing response
* execution time
* failures

Measures total downstream execution time.

---

# Authentication Flow

Header:

```http
Authorization: Bearer <token>
```

Authentication middleware:

* validates token
* validates expiry
* validates signature
* extracts claims

Creates:

```csharp
HttpContext.User
```

---

# Authorization Flow

Checks:

* roles
* policies
* permissions

Examples:

* AdminOnly
* UserOnly

---

# Middleware Order Importance

Correct:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

Why:

* Authorization requires authenticated user

---

# Controller Responsibility

Controllers should:

* receive HTTP request
* validate transport layer input
* call MediatR
* return response

Controllers should NOT contain:

* business logic
* DB logic
* infrastructure logic

---

# CQRS Runtime Flow

```text
Controller
↓
MediatR
↓
Command/Query
↓
Handler
↓
Repository/Service
↓
DbContext / Infrastructure
↓
Response
```

---

# Login Flow

```text
Client
↓
AuthController
↓
LoginCommand
↓
LoginHandler
↓
Repository
↓
BCrypt Verify
↓
JwtTokenService
↓
Generate Token
↓
Response
```

---

# Runtime Thinking Model

For every component ask:

```text
Who calls this?
When does it execute?
What middleware already ran?
What exists in HttpContext?
What services are injected?
What happens after _next()?
```
