# Request Lifecycle Understanding

# Full Request Flow

```text
HTTP Request
↓
Kestrel
↓
Middleware Pipeline
↓
Authentication
↓
Authorization
↓
Controller
↓
MediatR
↓
Handler
↓
Repository
↓
DbContext
↓
Database
↓
Response
```

---

# Important Understanding

Backend execution moves through layers sequentially.

Understanding:
- where validation occurs
- where DB interaction occurs
- where exceptions occur
- where logs occur

is critical for debugging and system design.

---

# Controller Responsibility

Controllers should:
- receive requests
- delegate work
- return responses

Controllers should NOT:
- contain heavy business logic
- directly orchestrate infrastructure

---

# Handler Responsibility

Handlers orchestrate:
- business workflows
- validation flow
- repository usage
- response creation

---

# Repository Responsibility

Repositories abstract:
- database interaction
- query execution
- persistence logic

---

# Most Important Understanding

Senior engineers understand execution FLOW,
not just syntax.