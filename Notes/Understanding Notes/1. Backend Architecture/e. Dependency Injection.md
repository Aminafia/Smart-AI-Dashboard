# Dependency Injection Understanding

# Files Involved

## API Layer

- `API/Program.cs`
- `API/Extensions/AuthExtensions.cs`
- `API/Extensions/HealthCheckExtensions.cs`

## Application Layer

- `Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`
- `Application/Common/Behaviors/ValidationBehavior.cs`

## Infrastructure Layer

- `Infrastructure/DependencyInjection.cs`
- `Infrastructure/Repositories/UserRepository.cs`
- `Infrastructure/Auth/JwtTokenService.cs`
- `Infrastructure/BackgroundServices/AIWorker.cs`
- `Infrastructure/Services/AIQueue.cs`
- `Infrastructure/Services/AIJobStore.cs`
- `Infrastructure/AI/AiService.cs`

---

# Layer

```text
Cross-cutting architectural system
```

Dependency Injection spans the entire backend because it manages object creation and dependency resolution across all layers.

---

# Purpose

Dependency Injection (DI) allows objects to receive dependencies from the framework instead of manually creating them.

Instead of:

```csharp
var repository = new UserRepository();
```

ASP.NET Core automatically injects dependencies.

---

# Core Principle

```text
Classes should depend on abstractions,
NOT concrete implementations.
```

Example:

```csharp
private readonly IUserRepository _userRepository;
```

NOT:

```csharp
private readonly UserRepository _userRepository;
```

This preserves:
- loose coupling
- testability
- clean architecture

---

# Where Dependency Injection Is Configured

Main registrations occur inside:

## API Layer

```text
API/Program.cs
```

and

## Infrastructure Layer

```text
Infrastructure/DependencyInjection.cs
```

---

# Infrastructure Composition Root

## File

```text
Infrastructure/DependencyInjection.cs
```

This file acts as the:

```text
Infrastructure Composition Root
```

Purpose:
- centralize infrastructure registrations
- keep `Program.cs` cleaner
- modularize service registration

---

# Database Registration

## File

```text
Infrastructure/DependencyInjection.cs
```

```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        configuration.GetConnectionString("DefaultConnection")));
```

---

# What Happens Internally

ASP.NET Core DI container now knows:

```text
Whenever AppDbContext requested,
create PostgreSQL EF Core DbContext.
```

---

# Repository Registration

```csharp
services.AddScoped<IUserRepository, UserRepository>();
```

Meaning:

```text
When IUserRepository requested,
inject UserRepository instance.
```

Important Clean Architecture understanding:

Application layer depends on:
- interface

Infrastructure provides:
- implementation

---

# AI Service Registration

```csharp
services.AddScoped<IAIProvider, OpenAIProvider>();

services.AddScoped<IAIService, AiService>();
```

Important abstraction chain:

```text
IAIService
↓
AiService
↓
IAIProvider
↓
OpenAIProvider
```

This architecture allows future provider replacement:
- OpenAI
- Gemini
- Claude
- local LLMs

without changing Application layer.

---

# JWT Service Registration

```csharp
services.AddScoped<IJwtTokenService, JwtTokenService>();
```

Purpose:
- Application depends only on abstraction
- Infrastructure implements token generation

Very important Clean Architecture principle.

---

# Cache Service Registration

```csharp
services.AddScoped<ICacheService, CacheService>();
```

Allows future cache replacement:
- Redis
- MemoryCache
- cloud cache providers

without modifying business workflows.

---

# Queue + Job Store Registration

```csharp
services.AddSingleton<IAIQueue, AIQueue>();

services.AddSingleton<IAIJobStore, AIJobStore>();
```

Important lifetime understanding:

Queues/store must survive entire application lifetime.

Using Singleton ensures:
- shared state
- single in-memory queue
- single in-memory job store

---

# Background Worker Registration

```csharp
services.AddHostedService<AIWorker>();
```

Registers:

```text
Infrastructure/BackgroundServices/AIWorker.cs
```

as continuously running background process.

---

# Constructor Injection Understanding

Most backend classes use:

```text
Constructor Injection
```

Example:

```csharp
public LoginCommandHandler(
    IUserRepository userRepository,
    IJwtTokenService jwtTokenService,
    ILogger<LoginCommandHandler> logger)
```

ASP.NET Core automatically resolves:
- repository
- token service
- logger

through DI container.

---

# Dependency Resolution Flow

Example login flow:

```text
AuthController
↓
IMediator injected
↓
LoginCommandHandler resolved
↓
IUserRepository resolved
↓
UserRepository injected
↓
AppDbContext injected
```

DI recursively resolves entire dependency graph automatically.

---

# Service Lifetimes

# Scoped

```csharp
services.AddScoped<...>()
```

One instance per HTTP request.

Examples:
- repositories
- DbContext
- AI services

Purpose:
- request isolation
- transaction safety

---

# Singleton

```csharp
services.AddSingleton<...>()
```

One instance for entire application lifetime.

Examples:
- queues
- in-memory job stores

Purpose:
- shared global state

---

# Hosted Services

```csharp
services.AddHostedService<AIWorker>();
```

Background service runs continuously while application alive.

---

# Important Scoped vs Singleton Understanding

Very important architecture rule:

```text
Singleton services cannot directly depend on Scoped services.
```

Why?

Because:
- Scoped services belong to HTTP request lifecycle
- Singleton survives forever

This causes lifetime conflicts.

---

# IServiceScopeFactory Understanding

## File

```text
Infrastructure/BackgroundServices/AIWorker.cs
```

`AIWorker`
is Singleton because HostedService lives entire app lifetime.

But:

```csharp
IAIService
```

is Scoped.

Solution:

```csharp
using var scope =
    _scopeFactory.CreateScope();
```

Then:

```csharp
scope.ServiceProvider
    .GetRequiredService<IAIService>();
```

creates temporary scoped service safely.

This is extremely important DI lifecycle understanding.

---

# ValidationBehavior Dependency Injection

## File

```text
Application/Common/Behaviors/ValidationBehavior.cs
```

DI injects validators automatically:

```csharp
IEnumerable<IValidator<TRequest>>
```

MediatR pipeline dynamically resolves validators based on request type.

Example:

```text
LoginCommand
↓
LoginCommandValidator
```

---

# Logger Injection Understanding

Example:

```csharp
ILogger<LoginCommandHandler>
```

ASP.NET Core automatically provides typed logger instance.

Benefits:
- structured logging
- contextual logs
- centralized logging infrastructure

---

# Options Pattern Understanding

## File

```text
Infrastructure/Auth/JwtSettings.cs
```

Configured inside:

```csharp
services.Configure<JwtSettings>(
    configuration.GetSection("JwtSettings")
);
```

Purpose:
- bind configuration into strongly typed object

Then injected using:

```csharp
IOptions<JwtSettings>
```

inside:

```text
JwtTokenService
```

This is standard enterprise .NET configuration pattern.

---

# Important Variables Understanding

| Variable          | Purpose                                 |
|-------------------|-----------------------------------------|
| `_userRepository` | repository abstraction                  |
| `_jwtTokenService`| token generation abstraction            |
| `_provider`       | AI provider abstraction                 |
| `_cache`          | cache abstraction                       |
| `_scopeFactory`   | create scoped services inside singleton |
| `services`        | DI registration container               |

---

# Production Relevance

Dependency Injection is critical in:
- enterprise APIs
- microservices
- distributed systems
- cloud-native applications

because it enables:
- modularity
- testing
- scalability
- maintainability
- abstraction boundaries

---

# Most Important Architectural Understanding

Dependency Injection controls:
- object creation
- dependency resolution
- service lifetimes
- abstraction wiring

The backend system is effectively built as a:

```text
dependency graph
```

managed automatically by ASP.NET Core.