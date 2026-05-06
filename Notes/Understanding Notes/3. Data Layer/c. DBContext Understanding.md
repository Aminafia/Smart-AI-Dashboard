# DbContext Understanding

# Files Involved

## Infrastructure Layer

- `Infrastructure/Data/AppDbContext.cs`
- `Infrastructure/Repositories/UserRepository.cs`
- `Infrastructure/DependencyInjection.cs`

## Core Layer

- `Core/Entities/User.cs`

## Configuration

- `appsettings.json`

---

# Layer

```text
Infrastructure Layer
```

`DbContext` belongs to Infrastructure because:
- database communication
- persistence
- ORM interaction
- SQL execution

are infrastructure concerns.

---

# What Is DbContext?

`DbContext` is:

```text
EF Core's main database gateway.
```

It acts as:
- database session
- entity tracker
- SQL translator
- unit of work

Very important backend concept.

---

# Current DbContext

# File

```text
Infrastructure/Data/AppDbContext.cs
```

```csharp
public class AppDbContext : DbContext
```

Current backend creates custom EF Core context:

```csharp
AppDbContext
```

for interacting with PostgreSQL database.

---

# Core Purpose Of DbContext

`DbContext` manages:

```text
C# Objects
↔
Database Tables
```

It allows backend code to work with:
- entities
- LINQ
- objects

instead of raw SQL.

---

# Current Database Flow

```text
Application Layer
↓
Repository
↓
AppDbContext
↓
EF Core
↓
PostgreSQL Database
```

`DbContext`
is the bridge between:
- application objects
- relational database

---

# DbContext Constructor Understanding

Inside:

```csharp
public AppDbContext(
    DbContextOptions<AppDbContext> options)
    : base(options)
{
}
```

Purpose:
- receive database configuration
- pass configuration into EF Core base class

---

# DbContextOptions Understanding

```csharp
DbContextOptions<AppDbContext>
```

contains:
- database provider
- connection string
- EF Core configuration
- logging settings
- tracking behavior

Very important EF Core configuration concept.

---

# Database Registration Flow

# File

```text
Infrastructure/DependencyInjection.cs
```

```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        configuration.GetConnectionString(
            "DefaultConnection"
        )));
```

Purpose:
- register DbContext into DI container
- configure PostgreSQL provider

---

# UseNpgsql Understanding

```csharp
UseNpgsql(...)
```

tells EF Core:

```text
Use PostgreSQL database provider.
```

Without provider:
EF Core cannot communicate with database.

---

# Connection String Flow

# File

```text
appsettings.json
```

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;..."
}
```

Contains:
- host
- port
- database
- username
- password

used by `AppDbContext`.

---

# DbSet Understanding

Inside `AppDbContext`:

```csharp
public DbSet<User> Users { get; set; }
```

Very important concept.

`DbSet<User>`
represents:

```text
Users database table
```

through C# abstraction.

---

# Entity Mapping Understanding

# File

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

Example:

| Entity Property | DB Column    |
|-----------------|--------------|
| `Email`         | Email        |
| `FullName`      | FullName     |
| `PasswordHash`  | PasswordHash |

---

# Repository + DbContext Relationship

# File

```text
Infrastructure/Repositories/UserRepository.cs
```

Repository injects:

```csharp
private readonly AppDbContext _context;
```

Purpose:
- execute DB operations through EF Core

Important architecture understanding:

```text
Repositories use DbContext.
Application layer does NOT.
```

This preserves Clean Architecture boundaries.

---

# Query Flow Understanding

Example:

```csharp
_context.Users.FirstOrDefaultAsync(
    u => u.Email == email
)
```

Execution flow:

```text
LINQ Expression
↓
DbContext
↓
EF Core translates to SQL
↓
PostgreSQL executes query
↓
Result mapped into User entity
```

---

# SQL Translation Understanding

EF Core internally generates SQL similar to:

```sql
SELECT *
FROM Users
WHERE Email = 'amina@test.com'
LIMIT 1;
```

Application never writes SQL manually here.

---

# Change Tracking Understanding

One of the MOST important DbContext concepts.

`DbContext` automatically tracks:
- entity creation
- entity updates
- entity deletion

This is called:

```text
Change Tracking
```

---

# AddAsync Understanding

Example:

```csharp
await _context.Users.AddAsync(user);
```

Important:

```text
Database INSERT has NOT happened yet.
```

Entity only becomes:

```text
Tracked as Added
```

inside DbContext memory.

---

# SaveChangesAsync Understanding

Actual DB interaction occurs during:

```csharp
await _context.SaveChangesAsync();
```

Very important concept.

At this moment:
EF Core:
- checks tracked entities
- generates SQL
- executes queries

---

# SaveChanges Flow

```text
Entity created
↓
DbContext tracks entity
↓
SaveChangesAsync()
↓
EF Core generates INSERT SQL
↓
Database updated
```

---

# DbContext Lifetime Understanding

Registered using:

```csharp
services.AddDbContext<AppDbContext>()
```

Default lifetime:

```text
Scoped
```

Meaning:

```text
One DbContext per HTTP request
```

Very important architecture concept.

---

# Why Scoped Lifetime Matters

Benefits:
- request isolation
- transaction safety
- thread safety
- clean unit-of-work behavior

DbContext should NEVER usually be Singleton.

---

# Unit Of Work Understanding

`DbContext` acts as:

```text
Unit Of Work
```

Meaning:
multiple DB operations can be grouped together before saving.

Example:

```text
Add user
Update logs
SaveChangesAsync()
```

All persisted together.

Very important enterprise backend concept.

---

# Async Database Operations

Current backend correctly uses:
- `FirstOrDefaultAsync`
- `AnyAsync`
- `ToListAsync`
- `SaveChangesAsync`

Benefits:
- non-blocking I/O
- scalability
- thread efficiency

Critical for high-scale APIs.

---

# Important Variables Understanding

| Variable           | Purpose               |
|--------------------|-----------------------|
| `_context`         | AppDbContext instance |
| `Users`            | DbSet<User>           |
| `options`          | DB configuration      |
| `user`             | tracked entity        |
| `connectionString` | PostgreSQL connection |

---

# DbContext vs Database

Important distinction:

| Component  | Responsibility        |
|------------|-----------------------|
| PostgreSQL | actual storage engine |
| DbContext  | ORM interaction layer |

DbContext does NOT store data itself.

Database stores actual persistent data.

---

# Production Relevance

DbContext is fundamental in:
- enterprise APIs
- SaaS platforms
- microservices
- cloud-native systems
- distributed systems

because EF Core is one of the most widely used .NET ORM systems.

---

# Most Important Architectural Understanding

`DbContext` acts as:

```text
Database Abstraction + Change Tracking Engine
```

It:
- manages entity state
- translates LINQ into SQL
- coordinates persistence
- controls DB interaction lifecycle

Current backend correctly keeps:
- DbContext in Infrastructure
- repositories abstracted
- Application layer independent

which preserves strong Clean Architecture boundaries.# DbContext Understanding

# Files Involved

## Infrastructure Layer

- `Infrastructure/Data/AppDbContext.cs`
- `Infrastructure/Repositories/UserRepository.cs`
- `Infrastructure/DependencyInjection.cs`

## Core Layer

- `Core/Entities/User.cs`

## Configuration

- `appsettings.json`

---

# Layer

```text
Infrastructure Layer
```

`DbContext` belongs to Infrastructure because:
- database communication
- persistence
- ORM interaction
- SQL execution

are infrastructure concerns.

---

# What Is DbContext?

`DbContext` is:

```text
EF Core's main database gateway.
```

It acts as:
- database session
- entity tracker
- SQL translator
- unit of work

Very important backend concept.

---

# Current DbContext

# File

```text
Infrastructure/Data/AppDbContext.cs
```

```csharp
public class AppDbContext : DbContext
```

Current backend creates custom EF Core context:

```csharp
AppDbContext
```

for interacting with PostgreSQL database.

---

# Core Purpose Of DbContext

`DbContext` manages:

```text
C# Objects
↔
Database Tables
```

It allows backend code to work with:
- entities
- LINQ
- objects

instead of raw SQL.

---

# Current Database Flow

```text
Application Layer
↓
Repository
↓
AppDbContext
↓
EF Core
↓
PostgreSQL Database
```

`DbContext`
is the bridge between:
- application objects
- relational database

---

# DbContext Constructor Understanding

Inside:

```csharp
public AppDbContext(
    DbContextOptions<AppDbContext> options)
    : base(options)
{
}
```

Purpose:
- receive database configuration
- pass configuration into EF Core base class

---

# DbContextOptions Understanding

```csharp
DbContextOptions<AppDbContext>
```

contains:
- database provider
- connection string
- EF Core configuration
- logging settings
- tracking behavior

Very important EF Core configuration concept.

---

# Database Registration Flow

# File

```text
Infrastructure/DependencyInjection.cs
```

```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        configuration.GetConnectionString(
            "DefaultConnection"
        )));
```

Purpose:
- register DbContext into DI container
- configure PostgreSQL provider

---

# UseNpgsql Understanding

```csharp
UseNpgsql(...)
```

tells EF Core:

```text
Use PostgreSQL database provider.
```

Without provider:
EF Core cannot communicate with database.

---

# Connection String Flow

# File

```text
appsettings.json
```

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;..."
}
```

Contains:
- host
- port
- database
- username
- password

used by `AppDbContext`.

---

# DbSet Understanding

Inside `AppDbContext`:

```csharp
public DbSet<User> Users { get; set; }
```

Very important concept.

`DbSet<User>`
represents:

```text
Users database table
```

through C# abstraction.

---

# Entity Mapping Understanding

# File

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

Example:

| Entity Property | DB Column    |
|-----------------|--------------|
| `Email`         | Email        |
| `FullName`      | FullName     |
| `PasswordHash`  | PasswordHash |

---

# Repository + DbContext Relationship

# File

```text
Infrastructure/Repositories/UserRepository.cs
```

Repository injects:

```csharp
private readonly AppDbContext _context;
```

Purpose:
- execute DB operations through EF Core

Important architecture understanding:

```text
Repositories use DbContext.
Application layer does NOT.
```

This preserves Clean Architecture boundaries.

---

# Query Flow Understanding

Example:

```csharp
_context.Users.FirstOrDefaultAsync(
    u => u.Email == email
)
```

Execution flow:

```text
LINQ Expression
↓
DbContext
↓
EF Core translates to SQL
↓
PostgreSQL executes query
↓
Result mapped into User entity
```

---

# SQL Translation Understanding

EF Core internally generates SQL similar to:

```sql
SELECT *
FROM Users
WHERE Email = 'amina@test.com'
LIMIT 1;
```

Application never writes SQL manually here.

---

# Change Tracking Understanding

One of the MOST important DbContext concepts.

`DbContext` automatically tracks:
- entity creation
- entity updates
- entity deletion

This is called:

```text
Change Tracking
```

---

# AddAsync Understanding

Example:

```csharp
await _context.Users.AddAsync(user);
```

Important:

```text
Database INSERT has NOT happened yet.
```

Entity only becomes:

```text
Tracked as Added
```

inside DbContext memory.

---

# SaveChangesAsync Understanding

Actual DB interaction occurs during:

```csharp
await _context.SaveChangesAsync();
```

Very important concept.

At this moment:
EF Core:
- checks tracked entities
- generates SQL
- executes queries

---

# SaveChanges Flow

```text
Entity created
↓
DbContext tracks entity
↓
SaveChangesAsync()
↓
EF Core generates INSERT SQL
↓
Database updated
```

---

# DbContext Lifetime Understanding

Registered using:

```csharp
services.AddDbContext<AppDbContext>()
```

Default lifetime:

```text
Scoped
```

Meaning:

```text
One DbContext per HTTP request
```

Very important architecture concept.

---

# Why Scoped Lifetime Matters

Benefits:
- request isolation
- transaction safety
- thread safety
- clean unit-of-work behavior

DbContext should NEVER usually be Singleton.

---

# Unit Of Work Understanding

`DbContext` acts as:

```text
Unit Of Work
```

Meaning:
multiple DB operations can be grouped together before saving.

Example:

```text
Add user
Update logs
SaveChangesAsync()
```

All persisted together.

Very important enterprise backend concept.

---

# Async Database Operations

Current backend correctly uses:
- `FirstOrDefaultAsync`
- `AnyAsync`
- `ToListAsync`
- `SaveChangesAsync`

Benefits:
- non-blocking I/O
- scalability
- thread efficiency

Critical for high-scale APIs.

---

# Important Variables Understanding

| Variable           | Purpose               |
|--------------------|-----------------------|
| `_context`         | AppDbContext instance |
| `Users`            | DbSet<User>           |
| `options`          | DB configuration      |
| `user`             | tracked entity        |
| `connectionString` | PostgreSQL connection |

---

# DbContext vs Database

Important distinction:

| Component  | Responsibility        |
|------------|-----------------------|
| PostgreSQL | actual storage engine |
| DbContext  | ORM interaction layer |

DbContext does NOT store data itself.

Database stores actual persistent data.

---

# Production Relevance

DbContext is fundamental in:
- enterprise APIs
- SaaS platforms
- microservices
- cloud-native systems
- distributed systems

because EF Core is one of the most widely used .NET ORM systems.

---

# Most Important Architectural Understanding

`DbContext` acts as:

```text
Database Abstraction + Change Tracking Engine
```

It:
- manages entity state
- translates LINQ into SQL
- coordinates persistence
- controls DB interaction lifecycle

Current backend correctly keeps:
- DbContext in Infrastructure
- repositories abstracted
- Application layer independent

which preserves strong Clean Architecture boundaries.