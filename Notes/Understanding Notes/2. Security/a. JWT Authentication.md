# JWT Authentication Understanding

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

# Login Flow

```text
Client
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
JWT Generation
↓
Response
```

---

# JWT Structure

JWT contains:
- claims
- issuer
- audience
- expiry
- signature

---

# Claims Understanding

Claims represent user identity data.

Examples:
- UserId
- Email
- Role

---

# Signature Understanding

JWT signed using:
- secret key
- HMAC SHA256

Purpose:
- prevent tampering
- verify authenticity

---

# Stateless Authentication

Server does NOT store sessions.

Token itself carries identity information.

This improves:
- scalability
- distributed system compatibility