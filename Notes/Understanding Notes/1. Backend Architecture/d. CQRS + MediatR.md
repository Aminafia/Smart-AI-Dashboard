# CQRS + MediatR Understanding

# Why MediatR?

Without MediatR:
- controllers become large
- business logic becomes scattered
- architecture becomes tightly coupled

With MediatR:

```text
Controller
→ MediatR
→ Handler
```

Controllers become thin.

---

# CQRS Principle

Separate:
- Commands
- Queries

---

# Commands

Commands modify state.

Examples:
- CreateUserCommand
- LoginCommand
- GenerateAICommand

---

# Queries

Queries fetch/read data.

Examples:
- GetUsersQuery
- GetAIStatusQuery

---

# Handler Responsibility

Handlers orchestrate workflows.

Example:

```text
LoginCommandHandler
1. fetch user
2. verify password
3. generate JWT
4. return response
```

---

# ValidationBehavior

Acts like middleware for MediatR.

Flow:

```text
Controller
→ MediatR
→ ValidationBehavior
→ Handler
```

Benefits:
- centralized validation
- cleaner handlers
- reusable rules

---

# Most Important Understanding

MediatR acts like:
- internal message bus
- request router
- orchestration layer