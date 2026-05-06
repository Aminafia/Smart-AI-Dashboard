# Dependency Injection Understanding

# What Is Dependency Injection?

Dependency Injection (DI) means:

```text
Objects receive dependencies from external system
instead of creating them manually.
```

Instead of:

```csharp
var repo = new UserRepository();
```

ASP.NET Core automatically provides dependencies.

---

# Why DI Is Important

Benefits:
- loose coupling
- scalability
- testability
- maintainability
- lifecycle management

---

# Constructor Injection

Example:

```csharp
public UsersController(IMediator mediator)
{
    _mediator = mediator;
}
```

Framework automatically injects implementation.

---

# Service Registration

Services registered in:

```text
Program.cs
DependencyInjection.cs
```

Example:

```csharp
services.AddScoped<IUserRepository, UserRepository>();
```

Meaning:

```text
When IUserRepository requested,
provide UserRepository instance.
```

---

# Service Lifetimes

## Singleton

One instance for entire app lifetime.

Examples:
- queues
- caches
- hosted services

---

## Scoped

One instance per HTTP request.

Examples:
- DbContext
- repositories

---

## Transient

New instance every resolution.

---

# Important Understanding

Incorrect lifetime usage can:
- cause memory leaks
- break DbContext
- create threading issues

---

# IServiceScopeFactory

BackgroundService is Singleton.

Scoped services cannot be injected directly.

Solution:

```text
Create DI scope manually
```

Used in AIWorker.