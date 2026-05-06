# Authorization + Policies Understanding

# Authentication vs Authorization

Authentication:
```text
Who are you?
```

Authorization:
```text
What are you allowed to access?
```

---

# [Authorize]

Protects endpoints.

Example:

```csharp
[Authorize]
```

Requires valid JWT token.

---

# Policy-Based Authorization

Policies define access rules.

Example:

```csharp
[Authorize(Policy = "AdminOnly")]
```

---

# Policy Registration

Policies configured in:

```csharp
AddAuthorization()
```

Example:

```csharp
options.AddPolicy("AdminOnly",
    policy => policy.RequireRole(Roles.Admin));
```

---

# Role-Based Access

JWT token contains role claim.

Example:

```text
Admin
User
```

Authorization middleware checks role before endpoint execution.

---

# Authorization Flow

```text
Request
↓
Authentication Middleware
↓
JWT validated
↓
Authorization Middleware
↓
Role/policy checked
↓
Controller access allowed/blocked
```

---

# Important Understanding

Authentication proves identity.

Authorization controls permissions.