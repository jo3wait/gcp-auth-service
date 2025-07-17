# gcp-auth-service
auth(login/register) service


# Image Thumbnail Service

A small full-stack sample on **Google Cloud** that lets users

* **Register / Log in** (JWT, Argon2id + pepper with Cloud KMS)
* **Upload** images ≤ 5 MB
* See a **list** with original & compressed sizes (KB) and a download link
* Eventarc triggers a resize-work to create the thumbnail

| Layer       | Tech                                          | Runtime                                                |
|-------------|-----------------------------------------------|--------------------------------------------------------|
| Front-end   | Angular 17 + Vite                             | Cloud Run (Nginx)                                      |
| Auth API    | .NET 8                                        | Cloud Run                                              |
| Image API   | .NET 8                                        | Cloud Run                                              |
| Resize API  | .NET 8                                        | Cloud Run                                              |
| Database    | Cloud SQL for SQL Server 2022 (private IP)    | Serverless VPC Connector                               |
| Storage     | Cloud Storage (bucket `thumbnail` `upload`)   | –                                                      |
| Secrets     | Secret Manager + Cloud KMS (HMAC SHA-256)     | –                                                      |
| CI/CD       | GitHub → Cloud Build → Cloud Run              | –                                                      |

---

## 1. Folder structure

```text
├─ AuthService/ .NET 8 API (login / register)
├─ ImageService/ .NET 8 API (list / upload)
├─ imageservice.client/ Angular 17 project
├─ ResizeWork/ .NET 8 API (/)
└─ README.md

AuthService/  (same as ImageService/)
├── AuthService.sln
├── AuthService.API/
│   ├── Controllers/
│   ├── Program.cs
│   └── appsettings.json
├── AuthService.Application/
│   └── (services, DTOs, interfaces)
├── AuthService.Domain/
│   └── (entities, value objects)
├── AuthService.Infrastructure/
│   └── (data, services)
└── Dockerfile

ResizeWork/
├── ResizeWork.sln
├── Controllers/
│   └── ResizeController.cs
├── Models/
│   └── (DTOs)
├── Services/
│   └── (services, interfaces, entities)
├── Program.cs
└── Dockerfile
```
---

## 2. Secrets & environment variables (Cloud Run)

| Key                          | Used by         | How to inject                                                                                 |
|------------------------------|-----------------|-----------------------------------------------------------------------------------------------|
| `Jwt__Key`                   | Auth, Image     | Secret `jwt-key`                                                                              |
| `ConnectionStrings__Default` | Auth, Image     | Secret `sql-conn` (`Server=10.x.x.x,1433;...`)                                                |
| `Kms__PasswordKeyPath`       | Auth            | Secret `kms-keypath`                                                                          |
| `GoogleCloud__BucketName`    | Image           | Secret `bucket-name` (`thumbnail-upload-bucket`)                                              |
| `Storage__Mode`              | Image           | Env `GCS`                                                                                     |
| `DB_CONN`                    | Resize          | Secret `sql-conn` (`Server=10.x.x.x,1433;...`)                                                |
| `TARGET_SIZE_KB`             | Resize          | Env `500`                                                                                     |
| `MIN_JPEG_QUALITY`           | Resize          | Env `60`                                                                                      |
| `THUMBS_BUCKET`              | Resize          | Env `thumbnail-thumbs-bucket`                                                                 |

---


## 3. API summary

| Method | Path                 | Auth       | Description        |
| ------ | -------------------- | ---------- | ------------------ |
| POST   | `/api/auth/register` | –          | New user           |
| POST   | `/api/auth/login`    | –          | Get JWT            |
| GET    | `/api/image/list`    | Bearer JWT | List files         |
| POST   | `/api/image/upload`  | Bearer JWT | Upload file        |
| GET    | *(public URL)*       | –          | Download thumbnail |
| POST   | `/resize`            | IAM (Internal)| Thumbnail       |

---

## 4. Local development

```bash
# 1. Dev SQL Server
docker run -d --name sql2022 \
  -e ACCEPT_EULA=Y -e SA_PASSWORD=xxxx \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest

# 2. Auth API
cd AuthService.API
dotnet run

# 3. Image API (Fake storage)
cd ImageService.API
dotnet run

# 4. Frontend
cd imageservice.client
VITE_AUTH_URL=http://localhost:5000 \
VITE_IMAGE_URL=http://localhost:5002 \
npm install
ng serve -o
```

---

## 5. CI / CD 
Push → Cloud Build builds 4 images → deploys 4 Cloud Run services → updates traffic.




