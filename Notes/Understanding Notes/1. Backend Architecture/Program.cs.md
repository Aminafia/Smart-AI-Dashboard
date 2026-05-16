# What is Program.cs?

`Program.cs` is the:

```text id="6igjlwm"
Application Composition Root
```

## What is a Composition Root?

A Composition Root is the central place where:

* all layers
* services
* middleware
* infrastructure
* dependencies

are connected together to build the final running application.

It is responsible for:

* configuring the application
* registering services
* configuring middleware pipeline
* wiring all architectural layers together
* building the runtime host

It is the startup engine of the ASP.NET Core application.

---

# Clean Architecture Layer Connection

```text id="ljh4u7"
API
↓
Application
↓
Core
↓
Infrastructure
```

## What are these layers?

### API Layer

Handles:

* HTTP requests
* controllers
* middleware
* API responses

Acts as entry point to the application.

---

### Application Layer

Contains:

* business workflows
* CQRS handlers
* validators
* application logic

Defines how the application behaves.

---

### Core Layer

Contains:

* entities
* interfaces
* business abstractions
* domain models

Represents pure business/domain definitions.

---

### Infrastructure Layer

Contains:

* database access
* repositories
* JWT implementation
* AI integrations
* Redis
* external services

Implements external technical concerns.

---

Only API layer knows all layers.

`Program.cs` is where:

* Application layer
* Infrastructure layer
* Middleware
* Authentication
* Logging
* Caching
* Observability

are composed together.

---

# High-Level Startup Lifecycle

```text id="jwvbwl"
Application Start
↓
Create Builder
↓
Load Configuration
↓
Register Services
↓
Configure Logging
↓
Configure Middleware
↓
Build App
↓
Start Web Server
↓
Listen for Requests
```

## What is Startup Lifecycle?

The startup lifecycle is the sequence ASP.NET Core follows to prepare the application before handling requests.

---

# CreateBuilder()

```csharp id="o1q7x9"
var builder = WebApplication.CreateBuilder(args);
```

## What is CreateBuilder()?

Creates the foundational ASP.NET Core application infrastructure.

Creates:

* DI container
* Configuration system
* Logging system
* Kestrel web server
* Routing system
* Middleware builder
* Hosting environment

---

# builder vs app

## builder

Used for:

```text id="t4h9s0"
Service registration phase
```

## What is Service Registration?

The process of registering dependencies into the DI container so ASP.NET can inject them automatically later.

Examples:

* AddControllers()
* AddAuthentication()
* AddDbContext()
* AddMediatR()

---

## app

Used for:

```text id="r9m5c7"
Request pipeline configuration phase
```

## What is Request Pipeline?

A chain of middleware components that process every incoming HTTP request.

Examples:

* UseAuthentication()
* UseAuthorization()
* UseMiddleware()

---

# Configuration System

```csharp id="3d9skh"
var configuration = builder.Configuration;
```

## What is Configuration System?

A centralized system for loading application settings from multiple sources.

Provides centralized configuration access.

Reads:

* appsettings.json
* appsettings.Development.json
* environment variables
* secrets
* command-line arguments

---

# Connection String

```csharp id="r1knvy"
var connectionString =
    configuration.GetConnectionString("DefaultConnection");
```

## What is a Connection String?

A configuration value containing database connection details:

* server
* database name
* username
* password
* port

Purpose:

* externalize infrastructure configuration
* avoid hardcoded DB values

Supports:

* Docker
* Kubernetes
* cloud deployments
* multiple environments

---

# Fail Fast Principle

```csharp id="42l2ut"
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(...);
}
```

## What is Fail Fast?

A design principle where the application crashes immediately when critical configuration is invalid instead of failing unpredictably later.

Purpose:

* crash immediately on invalid startup configuration
* avoid partially working application

Production engineering principle:

* detect infrastructure failure early

---

# Serilog Logging System

```csharp id="50pt4x"
Log.Logger = new LoggerConfiguration()
```

## What is Serilog?

A structured logging library for .NET applications.

Used for:

* structured logs
* centralized observability
* production diagnostics

Replaces default ASP.NET logging.

---

# Structured Logging

## What is Structured Logging?

Logging data in structured key-value format instead of plain text.

Bad:

```text id="l9r1wi"
Console.WriteLine("Error")
```

Good:

```json id="7ovls9"
{
  "Timestamp":"...",
  "Level":"Error",
  "CorrelationId":"...",
  "Message":"..."
}
```

Benefits:

* searchable logs
* distributed tracing
* centralized monitoring
* Grafana/Kibana support

---

# Log Enrichment

```csharp id="w8x9ph"
.Enrich.FromLogContext()
```

## What is Log Enrichment?

Adding extra metadata automatically to every log entry.

Allows middleware to inject:

* CorrelationId
* UserId
* Request metadata

into all downstream logs.

---

# Machine Enrichment

```csharp id="4ngvh7"
.Enrich.WithMachineName()
```

## What is Machine Enrichment?

Adds server/machine identity into logs.

Useful for:

* multi-server deployments
* Kubernetes
* cloud infrastructure

---

# Thread Enrichment

```csharp id="kzyw1v"
.Enrich.WithThreadId()
```

## What is Thread Enrichment?

Adds execution thread information into logs.

Useful for:

* async debugging
* concurrency tracing

---

# Console Sink

```csharp id="w3e4s6"
.WriteTo.Console()
```

## What is a Sink?

A destination where logs are written.

Outputs logs to terminal.

Useful for:

* local development
* Docker containers
* Kubernetes logs

---

# File Sink

```csharp id="5xw0nr"
.WriteTo.File(...)
```

## What is File Logging?

Persisting logs into physical files.

Creates persistent structured logs.

Features:

* rolling log files
* JSON structured output
* long-term diagnostics

---

# UseSerilog()

```csharp id="1k4qv8"
builder.Host.UseSerilog();
```

## What does UseSerilog() do?

Replaces the default ASP.NET logging engine with Serilog.

---

# Cross-Cutting Concerns

## What are Cross-Cutting Concerns?

Features that affect the entire application instead of one specific business feature.

Program.cs configures:

* Logging
* Authentication
* Authorization
* Exception handling
* Rate limiting
* Caching
* Observability
* Health monitoring

These affect the entire application.

---

# AddControllers()

```csharp id="awjpp4"
builder.Services.AddControllers();
```

## What are Controllers?

Classes that receive HTTP requests and return HTTP responses.

Registers:

* MVC framework
* routing
* model binding
* JSON serialization
* controller activation

---

# Dependency Injection Container

```csharp id="upztg0"
builder.Services
```

Represents:

```text id="rzb7po"
IServiceCollection
```

## What is Dependency Injection?

A design pattern where dependencies are automatically provided to classes instead of classes creating them manually.

Purpose:

* centralized dependency registry

---

# DI Example

```csharp id="gqzc7q"
services.AddScoped<IUserRepository, UserRepository>();
```

Meaning:

```text id="mzwq0y"
Whenever IUserRepository is requested,
inject UserRepository.
```

---

# Dependency Injection Benefits

* loose coupling
* testability
* centralized configuration
* abstraction-based architecture

---

# Swagger/OpenAPI

```csharp id="f1m82s"
builder.Services.AddSwaggerGen(...)
```

## What is Swagger/OpenAPI?

A standardized API documentation system.

Purpose:

* API documentation
* API discoverability
* testing endpoints

---

# JWT Swagger Integration

```csharp id="jlwm4h"
AddSecurityDefinition("Bearer", ...)
```

## What is JWT Integration in Swagger?

Allows authenticated API testing directly inside Swagger UI.

Adds JWT authentication support inside Swagger UI.

Allows:

* entering JWT token
* authenticated endpoint testing

---

# AddApplication()

```csharp id="5qjlwm"
builder.Services.AddApplication();
```

## What is AddApplication()?

A custom extension method that registers all Application-layer services.

Registers:

* MediatR
* Validators
* Pipeline behaviors
* Application services

---

# MediatR

## What is MediatR?

A .NET library implementing the Mediator pattern.

Used for:

* CQRS
* decoupled request handling

Runtime flow:

```text id="m38jye"
Controller
↓
Mediator.Send()
↓
Handler
```

---

# Validators

```csharp id="kxjlwm"
AddValidatorsFromAssembly(...)
```

## What are Validators?

Classes that validate incoming requests before business logic executes.

Automatically registers:

* LoginValidator
* RegisterValidator
* AI validators

---

# Pipeline Behaviors

Example:

```csharp id="4nn8by"
ValidationBehavior<TRequest,TResponse>
```

## What are Pipeline Behaviors?

Middleware-like components inside MediatR pipeline.

Acts like middleware for MediatR.

Runtime:

```text id="3krspn"
Controller
↓
Mediator
↓
ValidationBehavior
↓
Handler
```

---

# AddInfrastructure()

```csharp id="wzn3vc"
builder.Services.AddInfrastructure(configuration);
```

## What is AddInfrastructure()?

Registers infrastructure implementations and external technical services.

Registers:

* DbContext
* repositories
* JWT services
* AI services
* Redis services
* background workers
* external providers

Infrastructure layer contains implementations.

---

# Health Checks

```csharp id="3z7vjr"
builder.Services.AddCustomHealthChecks(...)
```

## What are Health Checks?

Endpoints that verify whether infrastructure dependencies are functioning correctly.

Purpose:

* infrastructure monitoring
* uptime monitoring
* dependency validation

Checks:

* PostgreSQL
* Redis
* external APIs

---

# HttpClient Factory

```csharp id="0sz80f"
builder.Services.AddHttpClient("AIClient")
```

## What is HttpClient Factory?

A managed system for creating optimized HttpClient instances.

Benefits:

* connection pooling
* DNS refresh handling
* resiliency integration

---

# Polly Resilience

```csharp id="cbg6e6"
.AddPolicyHandler(...)
```

## What is Polly?

A .NET resilience library for handling transient failures.

Adds:

* retry policies
* circuit breaker
* timeout policies

Purpose:

* resilient external API communication

---

# Retry Policy

## What is Retry Policy?

Automatically retries temporarily failed requests.

Example:

* temporary network failure
* temporary OpenAI outage

---

# Circuit Breaker

## What is Circuit Breaker?

Stops repeated failing external calls temporarily.

Prevents:

* cascading failures
* API overload

---

# Timeout Policy

```csharp id="8d7j1o"
Policy.TimeoutAsync<HttpResponseMessage>(5)
```

## What is Timeout Policy?

Cancels long-running requests after specified duration.

Prevents:

* hanging requests
* thread exhaustion

---

# Authentication

```csharp id="m0jlwm"
builder.Services.AddAuth(configuration);
```

## What is Authentication?

The process of verifying user identity.

Determines:

```text id="bzrpk4"
WHO is the user?
```

Processes:

* JWT validation
* token parsing
* claims creation

---

# Authorization

## What is Authorization?

The process of determining user permissions.

Determines:

```text id="jlwmow"
WHAT can the user access?
```

Uses:

* roles
* policies
* permissions

---

# ApiBehaviorOptions

```csharp id="kjlwm1"
SuppressModelStateInvalidFilter = true;
```

## What are ApiBehaviorOptions?

ASP.NET API behavior customization settings.

Disables automatic ASP.NET validation response.

Allows:

* custom validation pipeline
* FluentValidation handling
* standardized API responses

---

# Redis Cache

```csharp id="ovjlwm"
AddStackExchangeRedisCache(...)
```

## What is Redis?

An in-memory distributed cache database.

Used for:

* caching
* session storage
* performance optimization

---

# Rate Limiting

```csharp id="ljlwm2"
AddRateLimiter(...)
```

## What is Rate Limiting?

Restricting how many requests clients can make within a period.

Protects APIs from abuse.

---

# Concurrency Limiter

```csharp id="jlwm3m"
PermitLimit = 2
```

## What is Concurrency Limiting?

Restricts simultaneous active requests.

Allows only 2 concurrent requests.

---

# Fixed Window Limiter

```csharp id="jlwm4n"
PermitLimit = 5
Window = 10 seconds
```

## What is Fixed Window Rate Limiting?

Allows limited requests within a fixed time window.

Allows:

* 5 requests every 10 seconds

---

# Build()

```csharp id="jlwm5o"
var app = builder.Build();
```

## What does Build() do?

Creates finalized application runtime instance.

At this point:

* DI container finalized
* middleware builder finalized
* app ready to start

---

# Middleware Pipeline

## What is Middleware Pipeline?

A sequential chain of components that process every HTTP request and response.

Middleware executes in order.

---

# Swagger Middleware

```csharp id="jlwm6p"
app.UseSwagger();
app.UseSwaggerUI();
```

## What is Swagger Middleware?

Middleware that exposes Swagger documentation endpoints.

Enables:

* Swagger JSON endpoint
* Swagger UI

Development environment only.

---

# ExceptionHandlingMiddleware

## What is Exception Middleware?

A global error handling middleware.

Wraps entire downstream pipeline.

Purpose:

* centralized error handling
* standardized error responses

---

# CorrelationIdMiddleware

## What is Correlation ID?

A unique identifier attached to one request across the entire system.

Purpose:

* generate request correlation ID
* inject into Serilog context
* distributed request tracing

---

# RequestLoggingMiddleware

## What is Request Logging Middleware?

Middleware responsible for request lifecycle observability.

Purpose:

* log request start/end
* measure execution duration
* log failures

---

# HTTPS Redirection

```csharp id="jlwm7q"
app.UseHttpsRedirection();
```

## What is HTTPS Redirection?

Automatically redirects insecure HTTP requests to secure HTTPS.

Security feature.

---

# Authentication Middleware

```csharp id="jlwm8r"
app.UseAuthentication();
```

## What does Authentication Middleware do?

Validates incoming JWT tokens and creates authenticated user context.

Creates:

```csharp id="jlwm9s"
HttpContext.User
```

---

# Authorization Middleware

```csharp id="jlwm0t"
app.UseAuthorization();
```

## What does Authorization Middleware do?

Checks whether authenticated users can access requested resources.

Checks:

* roles
* policies
* permissions

Requires authenticated user.

---

# Middleware Order Importance

Correct order:

```text id="jlwm1u"
Authentication
↓
Authorization
```

Authorization depends on authenticated user.

---

# Rate Limiter Middleware

```csharp id="jlwm2v"
app.UseRateLimiter();
```

## What does Rate Limiter Middleware do?

Enforces configured request rate restrictions.

---

# Endpoint Mapping

```csharp id="jlwm3w"
app.MapControllers()
```

## What is Endpoint Mapping?

Connecting controller routes into ASP.NET routing system.

Maps controller routes into endpoint system.

---

# RequireRateLimiting()

```csharp id="jlwm4x"
.RequireRateLimiting("concurrency");
```

## What does RequireRateLimiting() do?

Applies selected rate limit policy to mapped endpoints.

Applies rate limiting policy to all controllers.

---

# Health Check Endpoint

```csharp id="jlwm5y"
app.MapCustomHealthChecks();
```

## What is Health Check Endpoint?

An HTTP endpoint exposing application health status.

Used by:

* Kubernetes
* Docker
* monitoring systems

---

# Run()

```csharp id="jlwm6z"
app.Run();
```

## What does Run() do?

Starts the web server and begins listening for incoming HTTP requests.

---

# Full Runtime Request Flow

```text id="jlwm7a"
Client Request
↓
Kestrel Server
↓
ExceptionMiddleware
↓
CorrelationMiddleware
↓
RequestLoggingMiddleware
↓
Authentication
↓
Authorization
↓
Controller
↓
MediatR
↓
ValidationBehavior
↓
Handler
↓
Repository
↓
DbContext
↓
PostgreSQL
↓
Response
↓
RequestLoggingMiddleware
↓
Client Response
```
