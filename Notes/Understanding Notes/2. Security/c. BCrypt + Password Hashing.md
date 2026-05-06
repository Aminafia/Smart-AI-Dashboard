# BCrypt + Password Hashing Understanding

# Password Security Principle

Passwords should NEVER be stored as plain text.

Instead:
- hash passwords
- store hashes

---

# Hashing vs Encryption

Encryption:
```text
reversible
```

Hashing:
```text
one-way
```

Passwords use hashing.

---

# BCrypt

BCrypt is password hashing algorithm.

Purpose:
- secure password storage
- resist brute-force attacks

---

# Password Verification

Passwords are NOT decrypted.

Flow:

```text
Input password
↓
BCrypt hashes input again
↓
Compare with stored hash
```

---

# Important Understanding

Even database admins should NOT know user passwords.

---

# Security Benefit

If DB leaked:
- attackers see hashes
- not actual passwords