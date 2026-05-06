# Repository Pattern Understanding

# Files Involved

## Application Layer

- `Application/Interfaces/IUserRepository.cs`
- `Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`
- `Application/Features/Users/Commands/CreateUser/CreateUserCommandHandler.cs`
- `Application/Features/Users/Queries/GetUsers/GetUsersQueryHandler.cs`

## Infrastructure Layer

- `Infrastructure/Repositories/UserRepository.cs`
- `Infrastructure/Data/AppDbContext.cs`

## Core Layer

- `Core/Entities/User.cs`

---

# Layer

```text
Application + Infrastructure
```

Repository pattern spans:
- Application layer (contracts/interfaces)
- Infrastructure layer (implementations)

This separation is one of the most important Clean Architecture principles.

---

# Purpose

Repository pattern abstracts database access from business workflows.

Application layer should NOT know:
- EF Core internals
- SQL queries
- PostgreSQL implementation details

Application should only know:

```csharp
IUserRepository
```

---

# Core Architectural Principle

Application depends on:

```text
abstractions/interfaces
```

Infrastructure provides:

```text
concrete implementations
```

---

# Current Repository Structure

# Application Layer

## File

```text
Application/Interfaces/IUserRepository.cs
```

Defines contract:

```csharp
public interface IUserRepository
{
    Task AddUserAsync(...)

    Task<List<User>> GetAllUsersAsync(...)

    Task<bool> EmailExistsAsync(...)

    Task<User?> GetUserByEmailAsync(...)
}
```

Application defines:
- WHAT operations exist

Application does NOT define:
- HOW DB access happens

---

# Infrastructure Layer

## File

```text
Infrastructure/Repositories/UserRepository.cs
```

Implements:

```csharp
IUserRepository
```

Infrastructure owns:
- EF Core
- LINQ queries
- DbContext usage
- persistence logic

---

# Why Repository Pattern Is Important

Without repositories:

```text
Handlers directly use DbContext
```

Problems:
- tight coupling
- poor testability
- infrastructure leakage into Application layer
- harder maintenance

With repositories:

```text
Handlers remain infrastructure-independent.
```

---

# Login Flow Using Repository

# Step 1 — Handler Depends On Abstraction

## File

```text
Application/Features/Auth/Commands/Login/LoginCommandHandler.cs
```

Handler injects:

```csharp
private readonly IUserRepository _userRepository;
```

Important understanding:

Application layer knows ONLY interface.

It does NOT know:
- EF Core
- PostgreSQL
- UserRepository implementation

---

# Step 2 — Dependency Injection Resolves Implementation

## File

```text
Infrastructure/DependencyInjection.cs
```

```csharp
services.AddScoped<IUserRepository, UserRepository>();
```

Meaning:

```text
When IUserRepository requested,
inject UserRepository.
```

---

# Step 3 — Handler Calls Repository

Inside handler:

```csharp
var user =
    await _userRepository.GetUserByEmailAsync(
        request.Email,
        cancellationToken
    );
```

Execution moves into:

```text
Infrastructure/Repositories/UserRepository.cs
```

---

# Repository Implementation Understanding

# File

```text
Infrastructure/Repositories/UserRepository.cs
```

Repository injects:

```csharp
private readonly AppDbContext _context;
```

Purpose:
- communicate with database through EF Core

---

# AppDbContext Understanding

## File

```text
Infrastructure/Data/AppDbContext.cs
```

```csharp
public DbSet<User> Users { get; set; }
```

Represents:

```text
Users database table
```

through EF Core abstraction.

---

# GetUserByEmailAsync Flow

Inside repository:

```csharp
return await _context.Users
    .FirstOrDefaultAsync(
        u => u.Email == email,
        cancellationToken
    );
```

Execution flow:

```text
UserRepository
↓
DbContext
↓
EF Core
↓
LINQ translated into SQL
↓
PostgreSQL executes query
↓
Result mapped into User entity
```

---

# Actual SQL Understanding

EF Core internally generates SQL similar to:

```sql
SELECT *
FROM Users
WHERE Email = 'amina@test.com'
LIMIT 1;
```

Returned row mapped into:

```csharp
User
```

entity.

---

# CreateUser Flow

# File

```text
Application/Features/Users/Commands/CreateUser/CreateUserCommandHandler.cs
```

---

# Step 1 — Email Check

Handler calls:

```csharp
await _userRepository.EmailExistsAsync(
    request.Email,
    cancellationToken
);
```

Repository executes:

```csharp
_context.Users.AnyAsync(...)
```

Purpose:
- verify duplicate emails

---

# Step 2 — User Entity Created

```csharp
var user = new User
{
    ...
}
```

---

# Step 3 — Persist User

Handler calls:

```csharp
await _userRepository.AddUserAsync(
    user,
    cancellationToken
);
```

---

# AddUserAsync Flow

Inside repository:

```csharp
await _context.Users.AddAsync(
    user,
    cancellationToken
);
```

Important understanding:

At this point:
user NOT yet inserted into database.

Entity only tracked by EF Core.

---

# SaveChangesAsync Understanding

Actual database insertion occurs during:

```csharp
await _context.SaveChangesAsync(
    cancellationToken
);
```

This is extremely important EF Core understanding.

---

# SQL Generated

EF Core internally generates:

```sql
INSERT INTO Users (...)
VALUES (...);
```

---

# GetAllUsers Flow

# File

```text
Application/Features/Users/Queries/GetUsers/GetUsersQueryHandler.cs
```

Handler calls:

```csharp
await _userRepository.GetAllUsersAsync(...)
```

Repository executes:

```csharp
_context.Users.ToListAsync(...)
```

EF Core fetches all rows from:
- Users table

Then maps rows into:

```csharp
List<User>
```

---

# DTO Mapping Understanding

Inside query handler:

```csharp
users.Select(user => new UserDto
{
    ...
})
```

Purpose:
- avoid exposing entities directly
- control API response shape
- separate persistence models from transport models

Very important architecture principle.

---

# Important Variables Understanding

| Variable          | Purpose                |
|-------------------|------------------------|
| `_userRepository` | repository abstraction |
| `_context`        | EF Core DbContext      |
| `Users`           | DbSet<User>            |
| `user`            | domain entity          |
| `request`         | incoming command/query |

---

# Dependency Flow

```text
Application Layer
↓
IUserRepository
↓
Infrastructure Layer
↓
UserRepository
↓
AppDbContext
↓
EF Core
↓
PostgreSQL
```

This preserves Clean Architecture dependency direction.

---

# Production Relevance

Repository pattern heavily used in:
- enterprise APIs
- layered architectures
- DDD systems
- microservices
- cloud-native systems

because it:
- isolates persistence
- improves maintainability
- improves testing
- supports future DB replacement

---

# Most Important Architectural Understanding

Repositories act as:

```text
Persistence abstraction boundary
```

Application workflows should NOT depend directly on:
- SQL
- EF Core
- database engines

This separation keeps architecture:
- modular
- scalable
- maintainable
- testable