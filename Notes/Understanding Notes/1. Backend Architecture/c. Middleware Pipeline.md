# Middleware Pipeline Understanding

# Files Involved

## API Layer

- `API/Program.cs`
- `API/Middlewares/CorrelationIdMiddleware.cs`
- `API/Middlewares/RequestLoggingMiddleware.cs`
- `API/Middlewares/ExceptionHandlingMiddleware.cs`

---

# Layer

```text
API Layer
```

Middleware belongs to the API layer because it handles HTTP request/response processing before execution reaches controllers.

---

# Purpose

Middleware forms the HTTP execution pipeline in ASP.NET Core.

Every incoming request flows through middleware sequentially.

Middleware is responsible for:
- request interception
- request modification
- response modification
- logging
- exception handling
- authentication
- authorization
- tracing
- rate limiting

---

# Core Middleware Pipeline

Configured inside:

```text
API/Program.cs
```

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();
```

---

# Important Execution Understanding

Middleware behaves like nested wrappers.

Execution flow:

```text
Request
→ Middleware A START
    → Middleware B START
        → Controller
    ← Middleware B END
← Middleware A END
Response
```

This is extremely important.

Middleware resumes execution AFTER the next middleware finishes.

---

# Actual Request Flow In Current Backend

```text
Incoming HTTP Request
↓
ExceptionHandlingMiddleware
↓
CorrelationIdMiddleware
↓
RequestLoggingMiddleware
↓
HTTPS Redirection
↓
Authentication
↓
Authorization
↓
Rate Limiter
↓
Controller
↓
MediatR
↓
Handler
↓
Database
↓
Response returns upward through middleware
```

---

# Step-by-Step Middleware Execution

# Step 1 — ExceptionHandlingMiddleware Executes First

## File

```text
API/Middlewares/ExceptionHandlingMiddleware.cs
```

Purpose:
- catch ALL unhandled exceptions globally
- standardize API error responses
- centralize logging

Why first?

Because all downstream exceptions must be caught here.

If middleware order incorrect:
exceptions may escape pipeline.

---

# Step 2 — CorrelationIdMiddleware Executes

## File

```text
API/Middlewares/CorrelationIdMiddleware.cs
```

Purpose:
- assign unique request identifier

Inside:

```csharp
var correlationId =
    context.Request.Headers[HeaderName].FirstOrDefault()
    ?? Guid.NewGuid().ToString();
```

If client already sends:

```text
X-Correlation-ID
```

reuse it.

Otherwise generate new GUID.

---

# HttpContext.Items Understanding

Correlation ID stored in:

```csharp
context.Items[HeaderName]
```

`HttpContext.Items`
acts as request-scoped storage.

Important:
- survives entire request lifecycle
- accessible by other middleware/controllers

---

# Response Header Injection

Middleware adds correlation ID to response:

```csharp
context.Response.Headers[HeaderName] = correlationId;
```

Benefits:
- client-side tracing
- distributed tracing
- debugging production systems

---

# Passing Execution Forward

Very important line:

```csharp
await _next(context);
```

`_next`
represents next middleware in pipeline.

Without this:
pipeline stops immediately.

---

# Step 3 — RequestLoggingMiddleware Executes

## File

```text
API/Middlewares/RequestLoggingMiddleware.cs
```

Purpose:
- log incoming requests
- log outgoing responses
- measure request duration
- trace failures

---

# Stopwatch Understanding

Middleware starts timing:

```csharp
var stopwatch = Stopwatch.StartNew();
```

Measures:
- latency
- execution duration
- API performance

Production systems heavily depend on request timing metrics.

---

# Correlation ID Retrieval

Middleware reads ID from previous middleware:

```csharp
var correlationId =
    context.Items["X-Correlation-ID"]?.ToString();
```

Important understanding:

Middleware can communicate using:
- `HttpContext.Items`
- headers
- request/response objects

---

# Incoming Request Logging

Logs:

```csharp
Log.Information(
    "[Middleware] Incoming Request..."
)
```

Includes:
- HTTP method
- path
- correlation ID

Example:

```text
POST /api/auth/login
```

---

# Passing Control To Next Middleware

```csharp
await _next(context);
```

Execution continues toward:
- authentication
- authorization
- controllers

---

# Response Logging

After controller finishes:

```csharp
stopwatch.Stop();
```

Middleware resumes execution.

Then logs:
- status code
- duration
- correlation ID

Very important middleware concept:

```text
Middleware executes BOTH:
before AND after next middleware.
```

---

# Exception Logging

If downstream exception occurs:

```csharp
catch (Exception ex)
```

Middleware logs:
- exception
- request path
- duration
- correlation ID

Then:

```csharp
throw;
```

rethrows exception upward to:

```text
ExceptionHandlingMiddleware
```

---

# Authentication Middleware

Configured through:

```text
API/Extensions/AuthExtensions.cs
```

Purpose:
- validate JWT tokens
- identify user

Checks:
- signature
- expiry
- issuer
- audience

If token invalid:
request blocked before controller.

---

# Authorization Middleware

Purpose:
- enforce policies/roles

Example:

```csharp
[Authorize(Policy = "AdminOnly")]
```

Authorization checks:
- claims
- roles
- policies

before allowing endpoint execution.

---

# Rate Limiting Middleware

Configured inside:

```text
API/Program.cs
```

```csharp
builder.Services.AddRateLimiter(...)
```

Purpose:
- prevent abuse
- limit concurrency
- protect APIs

Current implementation:
- concurrency limiter
- fixed window limiter

---

# Middleware Ordering Importance

Current order is architecturally correct.

Why?

| Middleware                         | Reason                   |
|------------------------------------|--------------------------|
| Exception first                    | catch everything         |
| Correlation before logging         | logs need IDs            |
| Logging before controllers         | track full lifecycle     |
| Authentication before authorization| identity required first  |
| Authorization before controllers   | protect endpoints        |

Incorrect ordering causes:
- missing logs
- broken tracing
- failed auth
- uncaught exceptions

---

# Important Variables Understanding

| Variable        | Purpose                         |
|-----------------|---------------------------------|
| `_next`         | next middleware delegate        |
| `context`       | current HTTP request/response   |
| `correlationId` | request trace identifier        |
| `stopwatch`     | request timing                  |
| `_logger`       | structured logging              |
| `HeaderName`    | correlation header constant     |

---

# Production Relevance

Middleware is heavily used in:
- microservices
- cloud-native APIs
- enterprise platforms
- distributed systems

Cross-cutting concerns belong in middleware because:
- reusable
- centralized
- scalable
- maintainable

---

# Most Important Architectural Understanding

Middleware creates:
- request orchestration pipeline
- centralized cross-cutting concern handling
- observability layer
- security enforcement layer

Understanding middleware deeply is essential for:
- backend engineering
- distributed systems
- production debugging
- performance optimization
- system observability