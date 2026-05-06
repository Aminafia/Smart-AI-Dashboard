# Correlation IDs Understanding

# Problem Without Correlation IDs

Large systems process many requests simultaneously.

Without request identifiers:
- logs become difficult to trace
- debugging becomes confusing

---

# Correlation ID Purpose

Assign unique ID per request.

Example:

```text
X-Correlation-ID
```

---

# Flow

```text
Incoming request
↓
Generate/read correlation ID
↓
Store in HttpContext.Items
↓
Attach to response
↓
Reuse in all logs
```

---

# Benefits

Correlation IDs help:
- distributed tracing
- debugging
- observability
- production monitoring

---

# Important Understanding

One request may generate:
- controller logs
- DB logs
- middleware logs
- AI logs

Correlation ID connects them together.