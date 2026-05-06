# JWT Authentication Understanding

# Files Involved

## API Layer

- `API/Controllers/AuthController.cs`
- `API/Extensions/AuthExtensions.cs`
- `API/Program.cs`

## Application Layer

- `Application/Features/Auth/Commands/Login/LoginCommand.cs`
- `Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`
- `Application/Interfaces/IJwtTokenService.cs`

## Infrastructure Layer

- `Infrastructure/Auth/JwtSettings.cs`
- `Infrastructure/Auth/JwtTokenService.cs`

## Core Layer

- `Core/Entities/User.cs`
- `Core/Constants/Roles.cs`

---

# Layer

```text
Cross-layer authentication system
```

JWT authentication spans:
- API
- Application
- Infrastructure
- Core

because authentication involves:
- HTTP requests
- workflows
- token generation
- identity claims

---

# Purpose

JWT authentication provides:

```text
Stateless Authentication
```

Meaning:
- server does NOT store sessions
- token itself contains user identity

Benefits:
- scalability
- distributed system compatibility
- microservice compatibility
- cloud-native authentication

---

# Authentication vs Authorization

## Authentication

```text
Who are you?
```

Verifies identity.

---

## Authorization

```text
What are you allowed to access?
```

Controls permissions after identity verified.

---

# JWT Login Flow

```text
Client
↓
POST /api/auth/login
↓
AuthController
↓
LoginCommand
↓
MediatR
↓
LoginCommandHandler
↓
UserRepository
↓
Database
↓
BCrypt password verification
↓
JwtTokenService
↓
JWT token generated
↓
ApiResponse<LoginResponse>
↓
Client receives token
```

---

# Step-by-Step Authentication Execution

# Step 1 — Client Sends Login Request

Client sends:

```http
POST /api/auth/login
```

JSON body:

```json
{
  "email": "amina@test.com",
  "password": "password123"
}
```

ASP.NET Core binds JSON into:

```csharp
LoginRequest request
```

inside:

```text
API/Controllers/AuthController.cs
```

---

# Step 2 — Controller Creates LoginCommand

Inside:

```csharp
AuthController.Login(LoginRequest request)
```

controller converts transport DTO into application command:

```csharp
var command = new LoginCommand
{
    Email = request.Email,
    Password = request.Password
};
```

Purpose:
- separate API transport model from application workflow model

---

# Step 3 — MediatR Routes Command

Controller calls:

```csharp
await _mediator.Send(command);
```

MediatR locates:

```text
LoginCommandHandler
```

inside:

```text
Application/Features/Auth/Commands/Login/
```

---

# Step 4 — ValidationBehavior Executes

Before handler executes:

```text
ValidationBehavior<TRequest, TResponse>
```

runs automatically.

Validator used:

```text
LoginCommandValidator
```

Validation rules:

```csharp
RuleFor(x => x.Email)

RuleFor(x => x.Password)
```

If validation fails:
handler never executes.

---

# Step 5 — LoginCommandHandler Executes

## File

```text
Application/Features/Auth/Commands/Login/LoginCommandHandler.cs
```

Dependencies injected:

```csharp
private readonly IUserRepository _userRepository;

private readonly IJwtTokenService _jwtTokenService;

private readonly ILogger<LoginCommandHandler> _logger;
```

Important understanding:

Application depends on abstractions,
NOT infrastructure implementations.

---

# Step 6 — Fetch User From Database

Handler calls:

```csharp
_userRepository.GetUserByEmailAsync(
    request.Email,
    cancellationToken
);
```

Execution moves into:

```text
Infrastructure/Repositories/UserRepository.cs
```

which executes:

```csharp
_context.Users.FirstOrDefaultAsync(...)
```

using EF Core.

---

# Step 7 — Password Verification

Inside handler:

```csharp
BCrypt.Net.BCrypt.Verify(
    request.Password,
    user.PasswordHash
);
```

Important security understanding:

Passwords are NEVER decrypted.

BCrypt:
- hashes incoming password again
- compares generated hash with stored hash

---

# UnauthorizedException Understanding

If:
- user missing
OR
- password invalid

handler throws:

```csharp
throw new UnauthorizedException(
    "Invalid email or password"
);
```

Exception eventually handled by:

```text
ExceptionHandlingMiddleware
```

which converts exception into standardized API response.

---

# Step 8 — JWT Token Generation

If password valid:

```csharp
_jwtTokenService.GenerateToken(user)
```

Execution moves into:

```text
Infrastructure/Auth/JwtTokenService.cs
```

---

# JwtSettings Understanding

## File

```text
Infrastructure/Auth/JwtSettings.cs
```

Configuration loaded from:

```text
appsettings.json
```

```json
"JwtSettings": {
  "Secret": "...",
  "Issuer": "...",
  "Audience": "...",
  "ExpiryMinutes": 60
}
```

---

# JWT Claims Understanding

Inside:

```csharp
GenerateToken(User user)
```

claims created:

```csharp
new Claim(
    ClaimTypes.NameIdentifier,
    user.Id.ToString()
)

new Claim(
    ClaimTypes.Email,
    user.Email
)

new Claim(
    ClaimTypes.Role,
    user.Role
)
```

Claims become user identity payload inside token.

---

# Secret Key Understanding

```csharp
Encoding.UTF8.GetBytes(
    _jwtSettings.Secret
)
```

Secret converted into symmetric signing key.

Purpose:
- sign token securely
- prevent tampering

---

# Signing Credentials

```csharp
new SigningCredentials(
    key,
    SecurityAlgorithms.HmacSha256
)
```

Uses:
- HMAC SHA256 signing algorithm

Purpose:
- verify token authenticity

---

# JWT Token Construction

```csharp
var token = new JwtSecurityToken(
    issuer: ...,
    audience: ...,
    claims: ...,
    expires: ...,
    signingCredentials: ...
);
```

JWT now contains:
- identity claims
- issuer
- audience
- expiry
- cryptographic signature

---

# Final Token Serialization

```csharp
return new JwtSecurityTokenHandler()
    .WriteToken(token);
```

Converts JWT object into encoded string.

---

# Step 9 — Handler Returns LoginResponse

Handler returns:

```csharp
new LoginResponse
{
    Token = ...,
    Email = user.Email
}
```

---

# Step 10 — Controller Returns ApiResponse

Controller wraps response:

```csharp
ApiResponse<LoginResponse>
    .SuccessResponse(...)
```

Final HTTP response returned to client.

---

# JWT Validation Flow

After login,
client sends token in:

```http
Authorization: Bearer <token>
```

---

# Authentication Middleware

Configured inside:

```text
API/Extensions/AuthExtensions.cs
```

```csharp
services.AddAuthentication(...)
    .AddJwtBearer(...)
```

---

# TokenValidationParameters Understanding

Middleware validates:

```csharp
ValidateIssuer = true

ValidateAudience = true

ValidateLifetime = true

ValidateIssuerSigningKey = true
```

Checks:
- token issuer
- token audience
- token expiry
- token signature

If invalid:
request blocked before controller.

---

# RoleClaimType Understanding

```csharp
RoleClaimType = ClaimTypes.Role
```

Allows authorization middleware to read role claims correctly.

---

# Important Variables Understanding

| Variable          | Purpose                      |
|-------------------|------------------------------|
| `request`         | login credentials            |
| `command`         | application workflow request |
| `_userRepository` | fetch user                   |
| `_jwtTokenService`| generate token               |
| `_jwtSettings`    | token configuration          |
| `claims`          | user identity payload        |
| `key`             | signing key                  |
| `creds`           | signing credentials          |
| `token`           | JWT object                   |

---

# Production Relevance

JWT heavily used in:
- REST APIs
- microservices
- cloud-native systems
- distributed systems
- mobile authentication
- SPA applications

because authentication becomes:
- scalable
- stateless
- portable

---

# Most Important Architectural Understanding

Authentication flow is intentionally separated across layers:

| Layer          | Responsibility          |
|----------------|-------------------------|
| API            | HTTP/auth middleware    |
| Application    | authentication workflow |
| Infrastructure | JWT implementation      |
| Core           | user identity/domain    |

This separation makes authentication:
- modular
- scalable
- maintainable
- testable