# Queue Architecture Understanding

# Queue Purpose

Queues decouple:

```text
HTTP requests
FROM
heavy background processing
```

---

# Important Principle

Controllers should NOT perform:
- long-running work
- expensive processing
- blocking operations

Instead:
- queue work
- process asynchronously

---

# Producer-Consumer Pattern

Producer:
```text
Controller
```

Consumer:
```text
Background Worker
```

---

# Queue Flow

```text
Client Request
↓
Controller creates AIJob
↓
Queue stores job
↓
Background worker dequeues job
↓
Job processed
```

---

# ConcurrentQueue

Used because:
- thread-safe
- supports concurrency
- lock-free operations

---

# Enterprise Usage

Queues used in:
- AI systems
- payment systems
- email systems
- event-driven systems
- distributed processing