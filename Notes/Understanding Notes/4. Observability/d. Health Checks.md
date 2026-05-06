# Health Checks Understanding

# Purpose

Health checks answer:

```text
Is the system healthy?
```

---

# Liveness Probe

Endpoint:

```text
/health/live
```

Checks:

```text
Is app alive?
```

---

# Readiness Probe

Endpoint:

```text
/health/ready
```

Checks:

```text
Can app serve traffic?
```

Usually checks:
- database
- Redis
- dependencies

---

# Detailed Health Endpoint

Returns structured diagnostics.

Example:

```json
{
  "status": "Healthy"
}
```

---

# PostgreSQL Health Check

Checks:
- DB connectivity
- database availability

---

# Kubernetes Relevance

Used heavily in:
- Kubernetes
- Docker orchestration
- cloud-native systems

Example:

```text
Readiness fails
→ traffic removed

Liveness fails
→ container restarted
```

---

# Important Understanding

Production systems require:
- monitoring
- recoverability
- operational visibility