# Repository Pattern Understanding

# Repository Purpose

Repositories abstract database access.

Flow:

```text
Handler
↓
IUserRepository
↓
UserRepository
↓
DbContext
↓
Database
```

---

# Why Repository Pattern?

Benefits:
- abstraction
- testability
- decoupling
- clean architecture

---

# Application vs Infrastructure

Application:
```text
defines contracts/interfaces
```

Infrastructure:
```text
implements actual DB logic
```

---

# Important Understanding

Application layer should NOT know:
- EF Core internals
- SQL implementation
- database engine details

It only knows interfaces.