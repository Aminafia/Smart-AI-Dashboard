# BCrypt + Password Hashing Understanding

# Files Involved

## Application Layer

- `Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`
- `Application/Features/Users/Commands/CreateUser/CreateUserCommandHandler.cs`

## Core Layer

- `Core/Entities/User.cs`

## Project Configuration

- `Application/Application.csproj`

---

# Layer

```text
Application Layer + Security Workflow
```

Password hashing belongs to authentication/security workflows because passwords must be securely stored and verified during user authentication.

---

# Purpose

Passwords should NEVER be stored as plain text.

Instead:
- passwords are hashed
- only hashes are stored in database

This protects users even if database is compromised.

---

# Plain Text Password Problem

BAD example:

```text
Password = "amina123"
```

stored directly in database.

If database leaked:
attackers immediately know all passwords.

Very dangerous.

---

# Hashing Understanding

Hashing converts input into irreversible fixed-length output.

Example:

```text
"password123"
↓
"$2a$11$..."
```

Important:

```text
Hashing is ONE-WAY.
```

Passwords cannot be decrypted back.

---

# Hashing vs Encryption

| Concept    | Purpose                  | Reversible? |
|------------|--------------------------|-------------|
| Encryption | secure transport/storage | YES         |
| Hashing    | password security        | NO          |

Passwords use hashing,
NOT encryption.

---

# BCrypt Understanding

Current backend uses:

```text
BCrypt.Net-Next
```

from:

```text
Application/Application.csproj
```

```xml
<PackageReference Include="BCrypt.Net-Next" />
```

BCrypt is designed specifically for password hashing.

---

# Why BCrypt Is Important

BCrypt protects against:
- brute-force attacks
- rainbow table attacks
- database leaks

because BCrypt intentionally slows hashing computation.

---

# User Registration Password Flow

# File

```text
Application/Features/Users/Commands/CreateUser/CreateUserCommandHandler.cs
```

---

# Step 1 — CreateUserCommand Received

Command contains:

```csharp
request.Password
```

This is RAW user password.

At this stage:
password should NEVER be persisted directly.

---

# Step 2 — BCrypt Hashing

Inside handler:

```csharp
PasswordHash = BCrypt.Net.BCrypt.HashPassword(
    request.Password
)
```

Important understanding:

```text
HashPassword()
```

does:
- salt generation
- secure hashing
- BCrypt formatting

automatically.

---

# Step 3 — User Entity Created

```csharp
var user = new User
{
    ...
    PasswordHash = ...
}
```

Inside:

```text
Core/Entities/User.cs
```

database stores:

```text
PasswordHash
```

NOT original password.

---

# Step 4 — Persist To Database

Handler calls:

```csharp
await _userRepository.AddUserAsync(
    user,
    cancellationToken
);
```

Database now stores only BCrypt hash.

---

# Login Password Verification Flow

# File

```text
Application/Features/Auth/Commands/Login/LoginCommandHandler.cs
```

---

# Step 1 — User Retrieved From Database

Handler fetches:

```csharp
var user =
    await _userRepository.GetUserByEmailAsync(...)
```

Returned entity contains:

```csharp
user.PasswordHash
```

NOT original password.

---

# Step 2 — Verify Password

Inside handler:

```csharp
var isPasswordValid =
    BCrypt.Net.BCrypt.Verify(
        request.Password,
        user.PasswordHash
    );
```

Important understanding:

BCrypt does NOT decrypt passwords.

Instead:
- hashes incoming password again
- compares generated hash with stored hash

---

# Password Verification Flow

```text
User enters password
↓
BCrypt hashes input again
↓
Compare with stored hash
↓
TRUE or FALSE returned
```

---

# Invalid Password Flow

If password invalid:

```csharp
throw new UnauthorizedException(
    "Invalid email or password"
);
```

Purpose:
- avoid exposing which field failed
- improve security

Very important security practice.

---

# Salt Understanding

BCrypt automatically adds:

```text
Salt
```

Salt = random value added before hashing.

Benefits:
- same password produces different hashes
- prevents rainbow table attacks

Example:

```text
password123
↓
Hash A

password123
↓
Hash B
```

Different hashes possible due to salt.

---

# Important Variables Understanding

| Variable           | Purpose                    |
|--------------------|----------------------------|
| `request.Password` | raw incoming password      |
| `user.PasswordHash`| stored BCrypt hash         |
| `isPasswordValid`  | BCrypt verification result |
| `PasswordHash`     | secure persisted password  |

---

# Security Architecture Understanding

Current backend architecture correctly separates:

| Layer | Responsibility |
|---|---|
| API | receive credentials |
| Application | authentication workflow |
| Infrastructure | persistence |
| Database | store hashes only |

Important:
database never stores raw passwords.

---

# Why Password Hashing Is Critical

If database compromised:

WITHOUT hashing:
- attackers immediately know passwords

WITH BCrypt:
- attackers only see hashes
- password cracking becomes expensive and slow

---

# Production Relevance

Password hashing critical in:
- enterprise applications
- SaaS platforms
- banking systems
- healthcare systems
- identity systems
- authentication platforms

because credential leaks are extremely dangerous.

---

# Most Important Architectural Understanding

Passwords should:
- NEVER be decrypted
- NEVER be logged
- NEVER be stored as plain text

Authentication systems should always:
- hash passwords during registration
- verify hashes during login
- separate authentication workflows cleanly

Current backend follows correct security practices for password handling.