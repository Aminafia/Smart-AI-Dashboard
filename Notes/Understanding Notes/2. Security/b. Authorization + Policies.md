# Authorization + Policies Understanding

# Files Involved

## API Layer

- `API/Extensions/AuthExtensions.cs`
- `API/Program.cs`

## Core Layer

- `Core/Constants/Roles.cs`

---

# Layer

```text
API Layer + Security Infrastructure
```

Authorization primarily belongs to the API/security boundary because it protects endpoint access before business logic executes.

---

# Purpose

Authorization controls:

```text
What authenticated users are allowed to access.
```

Authentication verifies identity.

Authorization verifies permissions.

---

# Authentication vs Authorization

## Authentication

```text
Who are you?
```

Verified using JWT token.

---

## Authorization

```text
What are you allowed to do?
```

Verified using:
- roles
- claims
- policies

---

# Authorization Flow

```text
Client Request
↓
JWT Authentication Middleware
↓
Token validated
↓
Claims extracted
↓
Authorization Middleware
↓
Policy/Role checked
↓
Controller access allowed or blocked
```

---

# Roles Understanding

## File

```text
Core/Constants/Roles.cs
```

```csharp
public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
}
```

Purpose:
- centralize role definitions
- avoid hardcoded strings everywhere

Better than:

```csharp
"Admin"
```

written repeatedly across codebase.

---

# JWT Role Claims

Inside:

```text
Infrastructure/Auth/JwtTokenService.cs
```

role claim added:

```csharp
new Claim(
    ClaimTypes.Role,
    user.Role
)
```

Important understanding:

Authorization depends on JWT claims.

Without role claims:
role-based authorization cannot work.

---

# Authentication Middleware

Configured inside:

```text
API/Extensions/AuthExtensions.cs
```

```csharp
services.AddAuthentication(...)
```

Purpose:
- validate JWT token
- build authenticated user identity

---

# Authorization Middleware

Configured inside:

```csharp
services.AddAuthorization(options =>
{
    ...
});
```

Purpose:
- define access rules
- enforce policies

---

# Policy Registration

Current backend policies:

```csharp
options.AddPolicy("AdminOnly",
    policy => policy.RequireRole(Roles.Admin));

options.AddPolicy("UserOnly",
    policy => policy.RequireRole(Roles.User));
```

Meaning:

| Policy | Required Role |
|---|---|
| `AdminOnly` | `Admin` |
| `UserOnly` | `User` |

---

# How Policies Work

When endpoint uses:

```csharp
[Authorize(Policy = "AdminOnly")]
```

ASP.NET Core automatically checks:

```text
Does JWT token contain:
ClaimTypes.Role = Admin ?
```

If YES:
request allowed.

If NO:
request blocked with:

```http
403 Forbidden
```

---

# [Authorize] Attribute Understanding

## Basic Authentication Requirement

```csharp
[Authorize]
```

Meaning:

```text
User must be authenticated.
```

No specific role required.

---

# Policy-Based Authorization

```csharp
[Authorize(Policy = "AdminOnly")]
```

Meaning:

```text
User must:
- be authenticated
AND
- satisfy policy requirements
```

---

# [AllowAnonymous] Understanding

Inside:

```text
API/Controllers/AuthController.cs
```

login endpoint uses:

```csharp
[AllowAnonymous]
```

Purpose:
- allow unauthenticated users
- bypass authorization checks

Without this:
login endpoint itself would require authentication.

---

# RoleClaimType Understanding

Inside:

```csharp
RoleClaimType = ClaimTypes.Role
```

Important because:

Authorization middleware must know:
which claim represents user role.

---

# Authorization Middleware Position

Inside:

```text
API/Program.cs
```

```csharp
app.UseAuthentication();

app.UseAuthorization();
```

Order is extremely important.

Correct order:

```text
Authenticate first
↓
Then authorize
```

Why?

Authorization requires authenticated identity first.

---

# Authorization Failure Flow

Example:

```text
User token role = User
Endpoint requires = AdminOnly
```

Flow:

```text
Authentication succeeds
↓
Authorization fails
↓
403 Forbidden returned
↓
Controller never executes
```

Important understanding:

Authorization blocks requests BEFORE business logic.

---

# Important Variables Understanding

| Variable          | Purpose                       |
|-------------------|-------------------------------|
| `user.Role`       | role stored in User entity    |
| `ClaimTypes.Role` | JWT role claim type           |
| `Roles.Admin`     | centralized role constant     |
| `Roles.User`      | centralized role constant     |
| `policy`          | authorization rule definition |

---

# Production Relevance

Authorization critical for:
- enterprise APIs
- admin dashboards
- SaaS platforms
- banking systems
- healthcare systems
- AI platform permissions

because systems must protect:
- sensitive data
- privileged operations
- administrative endpoints

---

# Important Architectural Understanding

Authentication and Authorization are intentionally separated.

| System         | Responsibility        |
|----------------|-----------------------|
| Authentication | identity verification |
| Authorization  | permission enforcement|

This separation improves:
- modularity
- scalability
- security architecture clarity

---

# Future Scalability Notes

Current backend uses:
- role-based authorization

Future evolution may include:
- claim-based authorization
- permission-based authorization
- resource-based authorization
- tenant-aware authorization
- policy handlers
- custom authorization requirements

Current architecture already supports future expansion cleanly.