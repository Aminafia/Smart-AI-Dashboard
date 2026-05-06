# Clean Architecture Understanding

# Core Principle

```text
Outer layers depend on inner layers.
Inner layers never depend on outer layers.
```

---

# Layers

## API Layer

Responsible for:
- controllers
- middleware
- authentication
- HTTP handling
- Swagger
- rate limiting

Should NOT contain business logic.

---

## Application Layer

Contains:
- CQRS
- MediatR handlers
- validators
- business workflows
- interfaces/contracts

Acts as orchestration layer.

---

## Infrastructure Layer

Contains implementation details:
- repositories
- EF Core
- JWT generation
- AI providers
- Redis
- background workers

Implements interfaces defined in Application.

---

## Core Layer

Contains:
- entities
- constants
- domain models

Core should remain independent and stable.

---

# Dependency Flow

```text
API
↓
Application
↓
Core

Infrastructure
↓
Application
↓
Core
```

---

# Most Important Understanding

Clean Architecture separates:
- business rules
FROM
- implementation details

This improves:
- maintainability
- scalability
- testing
- modularity