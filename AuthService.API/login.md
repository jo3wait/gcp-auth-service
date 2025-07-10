```mermaid
sequenceDiagram
    participant U as User (Browser)
    participant NG as Image Frontend
    participant AUTH as Auth Service (Cloud Run)
    participant DB as Cloud SQL (SQL Server)
    participant KMS as Cloud KMS
    U->>NG: Enter email & password
    NG->>AUTH: POST /api/auth/login { email, password }
    AUTH->>DB: SELECT * FROM USERS WHERE USER_ACCT = email
    alt unregistered
    DB-->>AUTH: (no row)
    AUTH-->>NG: 200 OK { success: false }
    else registered
    DB-->>AUTH: (rows)
    AUTH->>KMS: Verify password
    KMS-->>AUTH: Verify result
        alt success
            AUTH->>AUTH: Generate JWT
            AUTH-->>NG: 200 OK { success: true, token }
        else fail
            AUTH-->>NG: 200 OK { success: false }
        end
    end
    NG-->>U: Show result
```