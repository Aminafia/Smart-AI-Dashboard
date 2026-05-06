# Exception Handling Understanding

# Problem Without Centralized Handling

Without middleware:

```text
try-catch blocks everywhere
```

Problems:
- duplicated code
- inconsistent responses
- poor maintainability

---

# ExceptionHandlingMiddleware

Purpose:
- catch all exceptions globally
- standardize responses
- centralize logging

---

# Flow

```text
Request
↓
try
↓
Controller/DB/Services execute
↓
Exception occurs
↓
Middleware catches exception
↓
JSON response returned
```

---

# ValidationException Handling

Validation errors handled separately.

Benefits:
- cleaner API responses
- user-friendly validation messages

---

# Standardized API Responses

Middleware converts exceptions into:
- status code
- JSON structure
- error message

---

# Benefits

- centralized logging
- cleaner controllers
- consistent responses
- easier maintenance