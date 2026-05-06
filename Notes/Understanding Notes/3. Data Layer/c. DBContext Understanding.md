# DbContext Understanding

# What Is DbContext?

DbContext represents:

```text
Database session + change tracker + query translator
```

Acts as bridge between:
- C# application
- relational database

---

# Responsibilities

DbContext handles:
- database connection
- entity tracking
- query execution
- SaveChanges
- transactions

---

# DbSet

Example:

```csharp
_context.Users
```

Represents:
- Users table
- queryable entity collection

---

# Change Tracking

EF Core tracks entity changes automatically.

Example:

```text
Modified entities
New entities
Deleted entities
```

Tracked until:

```text
SaveChangesAsync()
```

---

# SaveChangesAsync

Very important concept:

```text
AddAsync() does NOT immediately insert into DB.
```

Actual SQL execution occurs during:

```text
SaveChangesAsync()
```

---

# LINQ Translation

Example:

```csharp
FirstOrDefaultAsync()
```

Gets translated into SQL query internally.

---

# Scoped Lifetime

DbContext registered as Scoped.

Meaning:
- one instance per request

Important for:
- thread safety
- proper transaction handling