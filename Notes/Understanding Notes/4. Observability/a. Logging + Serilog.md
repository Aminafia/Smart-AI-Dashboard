# Logging + Serilog Understanding

# Why Logging Matters

Production systems require observability.

Without logs:
- debugging becomes difficult
- failures hard to trace
- performance issues hidden

---

# Serilog

Serilog provides:
- structured logging
- file logging
- console logging
- searchable logs

Better than:

```text
Console.WriteLine()
```

---

# Structured Logging

Instead of:

```text
"User 5 logged in"
```

Use:

```text
"User {UserId} logged in"
```

Benefits:
- searchable
- filterable
- analytics-friendly

---

# Logging Levels

## Information

Normal operations.

## Warning

Unexpected but recoverable situations.

## Error

Failures/exceptions.

---

# Request Logging

RequestLoggingMiddleware logs:
- request start
- request end
- duration
- status code

---

# Performance Monitoring

Stopwatch used to measure latency.

Important for:
- optimization
- SLA monitoring
- API analytics