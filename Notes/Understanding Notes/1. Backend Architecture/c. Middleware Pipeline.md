# Middleware Pipeline Understanding

# Middleware Concept

Middleware behaves like nested wrappers.

Flow:

```text
Request
→ Middleware A
→ Middleware B
→ Controller
→ Middleware B resumes
→ Middleware A resumes
→ Response
```

---

# CorrelationIdMiddleware

Purpose:
- assign unique request ID

Why important:
- request tracing
- observability
- debugging distributed systems

---

# RequestLoggingMiddleware

Purpose:
- log request lifecycle
- measure latency
- trace failures

Important concepts:
- Stopwatch
- structured logging
- request timing

---

# ExceptionHandlingMiddleware

Purpose:
- centralize exception handling
- standardize API responses

Instead of:
```text
try-catch everywhere
```

Use:
```text
global exception middleware
```

Benefits:
- consistency
- cleaner controllers
- centralized logging

---

# Middleware Ordering Importance

Middleware order matters.

Example:

```text
Exception Middleware
↓
Correlation Middleware
↓
Logging Middleware
↓
Authentication
↓
Authorization
↓
Controllers
```

Incorrect order can:
- break auth
- miss logs
- fail tracing