# Polly + Fault Tolerance Understanding

# Files Involved

## Infrastructure Layer

- `Infrastructure/Resilience/AIResiliencePolicy.cs`
- `Infrastructure/DependencyInjection.cs`
- `Infrastructure/AI/Providers/OpenAIProvider.cs`

## API Layer

- `API/Program.cs`

## Project Configuration

- `Infrastructure/Infrastructure.csproj`
- `API/API.csproj`

---

# Layer

```text
Infrastructure Resilience Layer
```

Polly belongs to Infrastructure because:
- resiliency
- fault handling
- external service protection
- transient failure recovery

are infrastructure concerns.

---

# Purpose

Polly provides:

```text
Fault Tolerance
```

Meaning:
system can continue operating despite temporary failures.

---

# What Is Fault Tolerance?

Fault tolerance means:

```text
System survives failures gracefully.
```

Instead of:
- crashing
- failing immediately
- propagating instability

system:
- retries
- delays
- isolates failures
- protects resources

---

# Why Fault Tolerance Is Important

Modern applications depend heavily on:
- external APIs
- cloud services
- databases
- AI providers
- networks

Failures are inevitable.

Examples:
- API downtime
- network interruptions
- timeouts
- rate limiting
- temporary overload

Fault tolerance is essential in production systems.

---

# Polly Understanding

Current backend uses:

```text
Polly
```

from packages:

## Files

```text
Infrastructure/Infrastructure.csproj
API/API.csproj
```

Packages:

```xml
Polly

Microsoft.Extensions.Http.Polly
```

Purpose:
- resilient HttpClient execution
- retry handling
- circuit breaker handling
- timeout handling

---

# Current Polly Architecture

Current backend applies Polly policies to:

```text
AIClient HttpClient
```

using:

```csharp
services.AddHttpClient("AIClient")
```

---

# HttpClient Pipeline Understanding

Current flow:

```text
OpenAIProvider
↓
HttpClient
↓
Polly Policies
↓
External AI API
```

Important understanding:

```text
Polly wraps outgoing HTTP requests.
```

---

# AIResiliencePolicy Understanding

# File

```text
Infrastructure/Resilience/AIResiliencePolicy.cs
```

Purpose:
- centralize resilience logic
- modularize fault tolerance configuration

---

# Current Policies Implemented

Current backend uses:

| Policy          | Purpose                    |
|-----------------|----------------------------|
| Retry           | recover temporary failures |
| Circuit Breaker | stop overload propagation  |
| Timeout         | avoid hanging requests     |

This is strong enterprise resilience architecture.

---

# Retry Policy Understanding

Method:

```csharp
GetRetryPolicy()
```

Handles:

```csharp
HandleTransientHttpError()
```

Meaning:
retry temporary failures such as:
- 5xx responses
- network issues
- timeouts

---

# 429 Retry Handling

Additional condition:

```csharp
HttpStatusCode.TooManyRequests
```

Purpose:
- handle API rate limiting

Very important for:
- AI APIs
- cloud APIs
- SaaS integrations

---

# Exponential Backoff Understanding

Current retry delay:

```csharp
Math.Pow(2, retryAttempt)
```

Retry intervals:

| Attempt | Delay |
|---------|-------|
| 1       | 2 sec |
| 2       | 4 sec |
| 3       | 8 sec |

---

# Why Exponential Backoff Matters

Without backoff:

```text
Retries may overload failing systems.
```

Backoff:
- reduces pressure
- improves recovery chance
- prevents retry storms

Extremely important distributed systems concept.

---

# Circuit Breaker Understanding

Method:

```csharp
GetCircuitBreakerPolicy()
```

Purpose:
- stop repeated failing requests

Very important resilience pattern.

---

# Why Circuit Breakers Exist

BAD scenario:

```text
External AI API failing
↓
Application keeps sending requests
↓
Threads/resources exhausted
↓
System degradation spreads
```

Circuit breakers prevent this.

---

# Current Circuit Breaker Configuration

```csharp
CircuitBreakerAsync(
    handledEventsAllowedBeforeBreaking: 3,
    durationOfBreak: TimeSpan.FromSeconds(30)
)
```

Meaning:

After:
- 3 consecutive failures

Circuit opens for:
- 30 seconds

---

# Circuit Breaker States

| State     | Meaning          |
|-----------|------------------|
| Closed    | normal execution |
| Open      | requests blocked |
| Half-Open | testing recovery |

---

# Circuit Breaker Flow

```text
Requests fail repeatedly
↓
Circuit opens
↓
Requests blocked immediately
↓
Recovery period passes
↓
Half-open test request
↓
If successful → close circuit
```

---

# Timeout Policy Understanding

Current backend also uses:

```csharp
Policy.TimeoutAsync<HttpResponseMessage>(10)
```

or:

```csharp
TimeoutAsync(...5)
```

depending on registration.

Purpose:
- prevent hanging HTTP calls

---

# Why Timeouts Matter

Without timeouts:

```text
Threads may hang indefinitely.
```

Problems:
- thread exhaustion
- request pileups
- degraded performance

Timeouts protect application resources.

---

# Policy Chaining Understanding

Current backend chains policies:

```csharp
.AddPolicyHandler(retry)
.AddPolicyHandler(circuitBreaker)
.AddPolicyHandler(timeout)
```

Meaning:

```text
Multiple resilience layers
wrap same HTTP request.
```

Very important architecture concept.

---

# Complete HTTP Resilience Flow

```text
OpenAIProvider
↓
HttpClient request
↓
Timeout Policy
↓
Retry Policy
↓
Circuit Breaker
↓
External AI API
```

---

# OpenAIProvider Integration

# File

```text
Infrastructure/AI/Providers/OpenAIProvider.cs
```

Injected:

```csharp
factory.CreateClient("AIClient")
```

Meaning:
provider automatically inherits all Polly policies.

Very important DI + resilience integration concept.

---

# Fault Tolerance vs Error Handling

Important distinction:

| Concept         | Purpose           |
|-----------------|-------------------|
| Error Handling  | react to failures |
| Fault Tolerance | survive failures  |

Polly focuses on:
- resilience
- survivability
- graceful degradation

---

# Graceful Degradation Understanding

Current provider fallback:

```csharp
return "⚠️ AI service temporarily unavailable..."
```

Purpose:
- system continues functioning
- user receives controlled response
- avoids crashes

Very important resilience strategy.

---

# Important Variables Understanding

| Variable                             | Purpose               |
|--------------------------------------|-----------------------|
| `retryAttempt`                       | retry iteration       |
| `durationOfBreak`                    | circuit open duration |
| `handledEventsAllowedBeforeBreaking` | failure threshold     |
| `_httpClient`                        | resilient HTTP client |
| `factory`                            | HttpClientFactory     |

---

# Production Relevance

Polly heavily used in:
- microservices
- AI systems
- distributed systems
- cloud-native platforms
- enterprise APIs

because external systems fail frequently in production.

---

# Most Important Architectural Understanding

Polly creates:

```text
Resilient Infrastructure Boundaries
```

Current backend correctly protects:
- external AI APIs
- network communication
- async processing

using:
- retries
- circuit breakers
- timeouts

This creates strong:
- fault tolerance
- resilience
- production reliability foundations.