# EF Core Internals Understanding

# EF Core Purpose

EF Core is an ORM.

ORM:
```text
Object Relational Mapper
```

Maps:
- database rows
TO
- C# objects

---

# DbContext Understanding

DbContext represents:
- database session
- query translator
- entity tracker

---

# DbSet Understanding

Example:

```csharp
_context.Users
```

Represents:
- Users table
- queryable entity set

---

# SaveChangesAsync Understanding

Important concept:

```text
AddAsync() does NOT immediately insert into DB.
```

Actual DB interaction occurs during:

```text
SaveChangesAsync()
```

---

# LINQ to SQL

Example:

```csharp
FirstOrDefaultAsync()
```

Gets translated into SQL query.

Understanding:
- LINQ abstraction
AND
- actual SQL behavior

is important.