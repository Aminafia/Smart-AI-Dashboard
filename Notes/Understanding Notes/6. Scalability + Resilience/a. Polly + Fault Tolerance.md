# Polly + Fault Tolerance Understanding

# Fault Tolerance Principle

Production systems assume:

```text
Failures WILL happen
```

Goal:
- survive failures gracefully

---

# Polly

Polly provides resilience policies.

Used for:
- retries
- circuit breakers
- timeouts

---

# Retry Policy

Retries temporary failures automatically.

---

# Circuit Breaker

Purpose:
- stop repeated failing requests

Flow:

```text
Too many failures
↓
Circuit opens
↓
Requests temporarily blocked
↓
Recovery attempted later
```

Prevents cascading failures.

---

# Timeout Policy

Prevents requests hanging forever.

Important for:
- AI calls
- external APIs
- distributed systems

---

# Enterprise Importance

Fault tolerance critical for:
- cloud systems
- AI systems
- distributed services
- scalable APIs