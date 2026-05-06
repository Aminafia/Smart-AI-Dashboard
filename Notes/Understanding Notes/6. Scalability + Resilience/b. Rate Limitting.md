# Rate Limiting Understanding

# Files Involved

## API Layer

- `API/Program.cs`

## ASP.NET Core Infrastructure

- `Microsoft.AspNetCore.RateLimiting`

---

# Layer

```text
API Layer + Scalability Infrastructure
```

Rate limiting belongs near API boundary because it controls:
- incoming traffic
- request concurrency
- abuse prevention
- resource protection

before requests fully enter application workflows.

---

# Purpose

Rate limiting protects the backend from:

```text
Too many requests
```

It helps:
- prevent abuse
- improve stability
- control traffic spikes
- protect resources
- improve scalability

---

# Why Rate Limiting Is Important

Without rate limiting:

```text
A single client could overwhelm the server.
```

Problems:
- server overload
- thread exhaustion
- degraded performance
- denial-of-service risks
- AI cost explosion

Very important production concept.

---

# Current Backend Rate Limiting

Current backend implements TWO rate limiters:

| Limiter             | Purpose                       |
|---------------------|-------------------------------|
| Concurrency Limiter | limit simultaneous requests   |
| Fixed Window Limiter| limit request count over time |

---

# Package/Namespace Understanding

# File

```text
API/Program.cs
```

Using:

```csharp
using Microsoft.AspNetCore.RateLimiting;
```

ASP.NET Core provides built-in rate limiting middleware.

---

# Registration Flow

Inside:

```csharp
builder.Services.AddRateLimiter(...)
```

Purpose:
- register rate limiting services
- configure limiter strategies

---

# Current Limiter #1 — Concurrency Limiter

```csharp
options.AddConcurrencyLimiter(
    "concurrency",
    opt =>
```

Purpose:

```text
Limit number of simultaneous active requests.
```

---

# PermitLimit Understanding

```csharp
opt.PermitLimit = 2;
```

Meaning:

```text
Only 2 requests can execute simultaneously.
```

If third request arrives while:
- two requests still processing,

third request denied immediately.

---

# QueueLimit Understanding

```csharp
opt.QueueLimit = 0;
```

Meaning:

```text
Extra requests are NOT queued.
```

Instead:
- rejected immediately

Important scalability concept.

---

# Why Concurrency Limiting Matters

Protects against:
- thread exhaustion
- expensive operations
- long-running requests
- AI overload

Especially important for:
- AI systems
- expensive APIs
- CPU-intensive workloads

---

# Current Limiter #2 — Fixed Window Limiter

```csharp
options.AddFixedWindowLimiter(
    "fixed",
    opt =>
```

Purpose:

```text
Limit number of requests over time window.
```

---

# Fixed Window Configuration

```csharp
opt.PermitLimit = 5;

opt.Window = TimeSpan.FromSeconds(10);
```

Meaning:

```text
Only 5 requests allowed every 10 seconds.
```

---

# Fixed Window Example

Example timeline:

| Time               | Requests       |
|--------------------|----------------|
| 0–10 sec           | max 5 allowed  |
| 6th request        | rejected       |
| next window starts | counter resets |

---

# Why Fixed Window Limiting Matters

Protects against:
- spam requests
- brute-force attacks
- excessive API usage
- accidental traffic spikes

Very important API protection mechanism.

---

# Middleware Registration

# File

```text
API/Program.cs
```

```csharp
app.UseRateLimiter();
```

Enables rate limiting middleware in request pipeline.

---

# Endpoint Integration

Current backend applies:

```csharp
app.MapControllers()
   .RequireRateLimiting("concurrency");
```

Meaning:

```text
All controller endpoints use
"concurrency" limiter.
```

Very important endpoint-level configuration concept.

---

# Current Execution Flow

```text
Incoming Request
↓
Rate Limiter Middleware
↓
Check concurrency permits
↓
If allowed → continue pipeline
↓
If denied → reject request
```

---

# What Happens On Rejection

When limit exceeded:

ASP.NET Core automatically returns:

```http
429 Too Many Requests
```

Very important HTTP standard understanding.

---

# Why HTTP 429 Exists

429 specifically means:

```text
Client exceeded allowed request rate.
```

This helps:
- API clients
- frontend apps
- load balancers
- monitoring systems

understand throttling behavior.

---

# Rate Limiting vs Authentication

Important distinction:

| System         | Purpose                |
|----------------|------------------------|
| Authentication | identify users         |
| Authorization  | control permissions    |
| Rate Limiting  | control traffic volume |

Rate limiting protects infrastructure capacity.

---

# Concurrency vs Throughput Understanding

Important distinction.

| Limiter             | Controls              |
|---------------------|-----------------------|
| Concurrency Limiter | simultaneous requests |
| Fixed Window Limiter| requests over time    |

Both solve different scalability problems.

---

# Why AI Systems Need Strong Rate Limiting

AI operations may involve:
- expensive inference
- long-running processing
- external API costs
- GPU/CPU-heavy workloads

Without rate limiting:
- costs may explode
- system instability may occur

Very important AI platform architecture concept.

---

# Future Production Enhancements

Current backend demonstrates foundational architecture.

Production systems often extend with:
- per-user limits
- per-IP limits
- distributed rate limiting
- Redis-backed counters
- token bucket algorithms
- sliding windows

---

# Important Variables Understanding

| Variable        | Purpose                   |
|-----------------|---------------------------|
| `PermitLimit`   | max allowed operations    |
| `QueueLimit`    | queued request count      |
| `Window`        | limiter time period       |
| `"concurrency"` | limiter policy name       |
| `"fixed"`       | fixed-window policy name  |

---

# Production Relevance

Rate limiting heavily used in:
- SaaS APIs
- AI platforms
- cloud-native systems
- authentication systems
- payment systems
- enterprise APIs

because traffic control is critical for scalability.

---

# Most Important Architectural Understanding

Rate limiting creates:

```text
Traffic Protection Boundaries
```

It protects backend resources BEFORE:
- controllers
- handlers
- databases
- AI providers

become overloaded.

Current backend already demonstrates:
- concurrency protection
- request throttling
- API safeguarding
- scalability-aware infrastructure

which are strong production-oriented backend concepts.