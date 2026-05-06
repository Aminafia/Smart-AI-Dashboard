# Background Workers Understanding

# BackgroundService

Background workers continuously run while app is alive.

Example:

```text
while application running:
    dequeue job
    process
    retry if needed
```

---

# Purpose

Move heavy work outside HTTP request lifecycle.

---

# Enterprise Usage

Used in:
- email processing
- AI inference
- event systems
- Kafka consumers
- notifications
- payment systems

---

# IServiceScopeFactory Understanding

BackgroundService is Singleton.

Scoped services cannot be injected directly.

Solution:

```text
Create scope manually
```

This is important DI lifecycle understanding.

---

# Retry Logic

Distributed systems expect failures.

Examples:
- API failures
- network issues
- AI provider timeouts

Retry flow:

```text
RetryCount < MaxRetries
→ retry

Else
→ fail permanently
```