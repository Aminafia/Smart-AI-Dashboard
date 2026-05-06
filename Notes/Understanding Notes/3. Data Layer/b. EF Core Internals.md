# EF Core Internals Understanding

# Files Involved

## Infrastructure Layer

- `Infrastructure/Data/AppDbContext.cs`
- `Infrastructure/Repositories/UserRepository.cs`

## Core Layer

- `Core/Entities/User.cs`

## Application Layer

- `Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`
- `Application/Features/Users/Commands/CreateUser/CreateUserCommandHandler.cs`
- `Application/Features/Users/Queries/GetUsers/GetUsersQueryHandler.cs`

## Project Configuration

- `Infrastructure/Infrastructure.csproj`

---

# Layer

```text
Infrastructure Layer
```

EF Core belongs to Infrastructure because:
- database access
- persistence
- SQL generation
- ORM behavior

are implementation details.

---

# Purpose

EF Core acts as:

```text
ORM (Object Relational Mapper)
```

Purpose:
- map database tables to C# objects
- generate SQL from LINQ
- track entity changes
- manage database persistence

---

# ORM Understanding

ORM converts:

```text
Database Rows
↔
C# Objects
```

Example:

| PostgreSQL      | C#            |
|-----------------|---------------|
| Users table row | `User` entity |

---

# Current Database Stack

Current backend uses:

| Technology | Purpose             |
|------------|---------------------|
| PostgreSQL | database            |
| EF Core    | ORM                 |
| Npgsql     | PostgreSQL provider |

---

# EF Core Package References

## File

```text
Infrastructure/Infrastructure.csproj
```

Important packages:

```xml
Microsoft.EntityFrameworkCore

Npgsql.EntityFrameworkCore.PostgreSQL
```

Purpose:
- EF Core ORM engine
- PostgreSQL provider integration

---

# AppDbContext Understanding

# File

```text
Infrastructure/Data/AppDbContext.cs
```

```csharp
public class AppDbContext : DbContext
```

`DbContext`
represents:

```text
Database Session
+
Entity Tracker
+
Query Translator
```

Very important concept.

---

# DbContext Responsibilities

`AppDbContext`
handles:
- database connection
- LINQ translation
- entity tracking
- SaveChanges
- transactions
- SQL generation

---

# DbSet Understanding

Inside:

```csharp
public DbSet<User> Users { get; set; }
```

`DbSet<User>`
represents:

```text
Users database table
```

through C# abstraction.

---

# Entity Understanding

## File

```text
Core/Entities/User.cs
```

```csharp
public class User
{
    ...
}
```

EF Core maps:
- entity properties
TO
- database columns

---

# Database Registration Flow

## File

```text
Infrastructure/DependencyInjection.cs
```

```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        configuration.GetConnectionString("DefaultConnection")));
```

Purpose:
- register PostgreSQL EF Core DbContext
- configure DB provider

---

# Connection String Understanding

## File

```text
appsettings.json
```

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;..."
}
```

Contains:
- database host
- port
- database name
- username
- password

---

# Query Execution Flow

Example login flow:

```text
LoginCommandHandler
↓
IUserRepository
↓
UserRepository
↓
AppDbContext
↓
EF Core
↓
Generated SQL
↓
PostgreSQL
↓
Results mapped into User entity
```

---

# LINQ Understanding

Example:

```csharp
_context.Users.FirstOrDefaultAsync(
    u => u.Email == email
)
```

This is:

```text
LINQ query
```

NOT raw SQL.

---

# LINQ Translation Understanding

EF Core converts LINQ into SQL automatically.

Generated SQL similar to:

```sql
SELECT *
FROM Users
WHERE Email = 'amina@test.com'
LIMIT 1;
```

Important understanding:

```text
LINQ is translated,
NOT executed directly.
```

---

# FirstOrDefaultAsync Understanding

Example:

```csharp
FirstOrDefaultAsync(...)
```

Behavior:
- fetch first matching row
- return mapped entity
- return null if missing

---

# AnyAsync Understanding

Example:

```csharp
_context.Users.AnyAsync(
    u => u.Email == email
)
```

Purpose:
- check existence efficiently

Generated SQL similar to:

```sql
SELECT EXISTS(...)
```

Very efficient query pattern.

---

# ToListAsync Understanding

Example:

```csharp
_context.Users.ToListAsync()
```

Purpose:
- fetch all rows
- materialize into memory

Generated SQL:

```sql
SELECT * FROM Users;
```

---

# AddAsync Understanding

Example:

```csharp
await _context.Users.AddAsync(user)
```

Very important understanding:

```text
AddAsync() does NOT immediately insert into database.
```

Instead:
entity becomes tracked by EF Core.

---

# Change Tracker Understanding

EF Core internally tracks entity state.

Possible states:
- Added
- Modified
- Deleted
- Unchanged

Example:

```csharp
_context.Users.AddAsync(user)
```

marks entity as:

```text
Added
```

inside Change Tracker.

---

# SaveChangesAsync Understanding

Actual database interaction occurs during:

```csharp
await _context.SaveChangesAsync()
```

EF Core:
- detects tracked changes
- generates SQL
- executes SQL

---

# Insert Flow Understanding

```text
User entity created
↓
AddAsync()
↓
Entity tracked
↓
SaveChangesAsync()
↓
INSERT SQL generated
↓
PostgreSQL executes insert
```

---

# Mapping Understanding

Database rows mapped into:

```csharp
User
```

entities automatically.

Example:

| DB Column | Entity Property |
|-----------|-----------------|
| Email     | `User.Email`    |
| FullName  | `User.FullName` |

---

# Async Database Operations

Current backend uses:
- `FirstOrDefaultAsync`
- `ToListAsync`
- `SaveChangesAsync`
- `AnyAsync`

Purpose:
- avoid blocking threads
- improve scalability
- support concurrent requests

Very important for enterprise APIs.

---

# DbContext Lifetime Understanding

Registered as:

```csharp
services.AddDbContext<AppDbContext>()
```

Default lifetime:

```text
Scoped
```

Meaning:
- one DbContext per HTTP request

Benefits:
- request isolation
- transaction safety
- thread safety

---

# Important Variables Understanding

| Variable           | Purpose                     |
|--------------------|-----------------------------|
| `_context`         | EF Core DbContext           |
| `Users`            | DbSet<User>                 |
| `user`             | tracked entity              |
| `options`          | DbContext configuration     |
| `connectionString` | DB connection configuration |

---

# Production Relevance

EF Core heavily used in:
- enterprise APIs
- SaaS platforms
- microservices
- cloud-native applications

because it:
- accelerates development
- abstracts SQL
- supports migrations
- integrates well with .NET ecosystem

---

# Important Architectural Understanding

EF Core belongs ONLY in Infrastructure layer.

Application layer should NOT directly depend on:
- DbContext
- LINQ queries
- EF Core APIs

Application should depend on:
- repository abstractions

This preserves Clean Architecture boundaries.

---

# Most Important Understanding

EF Core acts as:

```text
Translation + Tracking Engine
```

It:
- translates LINQ into SQL
- tracks entity changes
- synchronizes C# objects with database state

Understanding this internally is extremely important for:
- backend engineering
- performance optimization
- debugging
- database architecture