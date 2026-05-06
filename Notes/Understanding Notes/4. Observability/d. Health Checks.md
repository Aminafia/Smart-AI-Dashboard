# Health Checks Understanding

# Files Involved

## API Layer

- `API/Extensions/HealthCheckExtensions.cs`
- `API/Program.cs`

## Project Configuration

- `API/API.csproj`

## Infrastructure Dependency

- PostgreSQL
- ASP.NET Core Health Checks

---

# Layer

```text
API Layer + Observability Infrastructure
```

Health checks belong to observability because they expose application/system health information to:
- developers
- monitoring systems
- orchestration platforms

---

# Purpose

Health checks allow systems to determine:

```text
Is the application healthy and operational?
```

This is critical in production systems for:
- uptime monitoring
- Kubernetes orchestration
- load balancers
- cloud deployments
- automated recovery systems

---

# Why Health Checks Are Important

Without health checks:

```text
Systems cannot automatically determine:
- app health
- DB connectivity
- service readiness
```

Production orchestration platforms depend heavily on health endpoints.

---

# Health Check Package

## File

```text
API/API.csproj
```

```xml
<PackageReference Include="AspNetCore.HealthChecks.NpgSql" />
```

Purpose:
- PostgreSQL-specific health checks

Allows backend to verify:
- DB connectivity
- PostgreSQL availability

---

# Health Check Registration Flow

# File

```text
API/Program.cs
```

```csharp
builder.Services.AddCustomHealthChecks(
    connectionString
);
```

Purpose:
- register application health checks
- modularize health configuration

---

# HealthCheckExtensions Understanding

# File

```text
API/Extensions/HealthCheckExtensions.cs
```

Purpose:
- keep `Program.cs` cleaner
- centralize health check configuration
- modularize observability setup

---

# AddCustomHealthChecks Flow

Method:

```csharp
public static IServiceCollection
    AddCustomHealthChecks(...)
```

Registers health checks into ASP.NET Core DI container.

---

# HealthChecks Builder Understanding

```csharp
services.AddHealthChecks()
```

Creates health check pipeline builder.

Additional checks chained afterward.

---

# PostgreSQL Health Check

```csharp
.AddNpgSql(
    connectionString,
    name: "PostgreSQL",
    tags: new[] { "ready" }
)
```

Purpose:
- verify PostgreSQL connectivity
- confirm DB availability

---

# What PostgreSQL Check Actually Tests

Health check attempts:
- database connection
- PostgreSQL responsiveness

If connection succeeds:

```text
Healthy
```

If DB unreachable:

```text
Unhealthy
```

---

# Self Health Check

```csharp
.AddCheck(
    "Self",
    () => HealthCheckResult.Healthy(),
    tags: new[] { "live" }
)
```

Purpose:
- verify application process alive

This does NOT check:
- database
- external services

It only verifies:
- application itself running

---

# Health Check Tags Understanding

Current backend uses:
- `"live"`
- `"ready"`

These are extremely important production concepts.

---

# Liveness Check Understanding

```text
Is application process alive?
```

Endpoint:

```http
/health/live
```

If this fails:
system/container likely crashed.

---

# Readiness Check Understanding

```text
Is application ready to serve traffic?
```

Endpoint:

```http
/health/ready
```

Checks:
- PostgreSQL connectivity

If DB unavailable:
application should NOT receive traffic.

Very important cloud-native concept.

---

# Health Endpoint Mapping

# File

```text
API/Extensions/HealthCheckExtensions.cs
```

Mapped inside:

```csharp
endpoints.MapHealthChecks(...)
```

---

# /health/live Endpoint

```csharp
Predicate = r => r.Tags.Contains("live")
```

Runs ONLY:
- liveness checks

---

# /health/ready Endpoint

```csharp
Predicate = r => r.Tags.Contains("ready")
```

Runs ONLY:
- readiness checks

---

# /health/details Endpoint

Custom endpoint:

```http
/health/details
```

Returns detailed JSON response.

---

# Custom ResponseWriter Understanding

Inside:

```csharp
ResponseWriter = async (context, report) =>
{
    ...
}
```

Purpose:
- customize health check output
- return structured JSON response

---

# HealthReport Understanding

Variable:

```csharp
report
```

Contains:
- overall status
- individual check results
- exceptions/errors

---

# Detailed Health Response

Generated JSON:

```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "PostgreSQL",
      "status": "Healthy",
      "error": null
    }
  ]
}
```

Very useful for:
- dashboards
- monitoring systems
- debugging infrastructure issues

---

# Health Check Execution Flow

```text
Client/System
↓
GET /health/details
↓
ASP.NET Health Check Pipeline
↓
PostgreSQL check executes
↓
Results aggregated
↓
JSON response returned
```

---

# Health Checks vs Monitoring

Important distinction:

| System        | Purpose                   |
|---------------|---------------------------|
| Health checks | current operational state |
| Monitoring    | long-term metrics/trends  |

Health checks answer:

```text
Can system currently operate?
```

---

# Production Usage

Health checks heavily used by:
- Kubernetes
- Docker
- Azure App Services
- AWS ECS/EKS
- Load balancers
- Monitoring platforms

---

# Kubernetes Example

Kubernetes may call:

```http
/health/live
```

If failed:
container restarted automatically.

Kubernetes may call:

```http
/health/ready
```

If failed:
traffic removed from instance.

Extremely important cloud-native architecture concept.

---

# Important Variables Understanding

| Variable           | Purpose                         |
|--------------------|---------------------------------|
| `connectionString` | PostgreSQL connection           |
| `report`           | aggregated health status        |
| `context`          | HTTP response context           |
| `checks`           | individual health check results |
| `status`           | overall health state            |

---

# Why Health Checks Matter In AI Systems

AI platforms often depend on:
- databases
- vector DBs
- Redis
- AI providers
- queues
- workers

Health checks become critical for:
- resilience
- orchestration
- uptime monitoring

Current backend already has good observability foundations.

---

# Most Important Architectural Understanding

Health checks create:

```text
Machine-readable operational visibility
```

This allows infrastructure systems to:
- monitor applications
- automate recovery
- manage traffic routing
- detect outages

Current backend correctly separates:
- liveness
- readiness
- detailed diagnostics

which is strong production-oriented architecture.