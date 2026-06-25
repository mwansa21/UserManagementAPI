# User Management API

A **.NET 8 Web API** for managing users, built as part of the *Building a Simple API with Copilot* assignment. The project covers all five rubric areas: GitHub repo, CRUD endpoints, Copilot-assisted debugging, input validation, and middleware.

---

## Table of Contents
1. [Project Structure](#project-structure)  
2. [Features](#features)  
3. [Getting Started](#getting-started)  
4. [API Endpoints](#api-endpoints)  
5. [Authentication](#authentication)  
6. [Middleware](#middleware)  
7. [Validation](#validation)  
8. [GitHub Copilot Usage](#github-copilot-usage)  

---

## Project Structure

```
UserManagementAPI/
├── Controllers/
│   ├── AuthController.cs       # POST /api/auth/login → JWT token
│   └── UsersController.cs      # Full CRUD for /api/users
├── Data/
│   └── UserStore.cs            # In-memory data store (seeded with 3 users)
├── Middleware/
│   ├── RequestLoggingMiddleware.cs     # Logs every request + response time
│   └── GlobalExceptionMiddleware.cs   # Catches unhandled exceptions
├── Models/
│   └── User.cs                 # User entity + UserDto + LoginRequest
├── Program.cs                  # App entry point, DI, middleware pipeline
├── appsettings.json
└── UserManagementAPI.csproj
```

---

## Features

| Rubric Item | Implementation |
|---|---|
| ✅ GitHub Repository | Push this folder to a new GitHub repo |
| ✅ CRUD Endpoints | GET / GET by ID / POST / PUT / DELETE on `/api/users` |
| ✅ Copilot Debugging | See [Copilot Usage](#github-copilot-usage) section |
| ✅ Validation | Data annotations + custom email-uniqueness check |
| ✅ Middleware | Request logging + global exception handling + JWT auth |

---

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)

### Run locally
```bash
cd UserManagementAPI
dotnet restore
dotnet run
```

Navigate to **http://localhost:5000** — Swagger UI loads automatically.

---

## API Endpoints

### Auth
| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/login` | None | Get a JWT token |

**Demo credentials**

| Username | Password | Role |
|---|---|---|
| `admin` | `Admin@123` | Admin |
| `user` | `User@123` | User |

---

### Users
| Method | Route | Auth Required | Description |
|---|---|---|---|
| GET | `/api/users` | No | List all active users |
| GET | `/api/users/{id}` | No | Get a user by ID |
| POST | `/api/users` | **Yes** | Create a new user |
| PUT | `/api/users/{id}` | **Yes** | Update an existing user |
| DELETE | `/api/users/{id}` | **Yes** | Soft-delete a user |

#### Example: Create user (POST `/api/users`)
```json
{
  "name": "Dana Lee",
  "email": "dana@example.com",
  "role": "User",
  "phoneNumber": "555-0200"
}
```

#### Valid roles
`Admin` | `User` | `Moderator`

---

## Authentication

The API uses **JWT Bearer tokens**.

1. Call `POST /api/auth/login` with valid credentials.  
2. Copy the `token` from the response.  
3. In Swagger UI, click **Authorize** and enter `Bearer <token>`.  
4. All `POST`, `PUT`, and `DELETE` requests will now succeed.

---

## Middleware

### 1. `RequestLoggingMiddleware`
Logs every HTTP request and response to the console:
```
[REQUEST]  POST /api/users | TraceId: 0HNABC
[RESPONSE] POST /api/users => 201 (12 ms) | TraceId: 0HNABC
```
Responses taking **> 500 ms** are logged at `Warning` level.

### 2. `GlobalExceptionMiddleware`
Catches any unhandled exception and returns a clean JSON response instead of leaking a stack trace:
```json
{
  "error": "An unexpected error occurred.",
  "traceId": "0HNABC"
}
```
In `Development` mode the `detail` field also includes the exception message.

### 3. JWT Authentication Middleware
Built-in `AddAuthentication().AddJwtBearer(...)` validates tokens on every protected route.

---

## Validation

Validation is layered:

| Layer | What it checks |
|---|---|
| Data annotations (`[Required]`, `[EmailAddress]`, `[StringLength]`, `[RegularExpression]`, `[Phone]`) | Format & presence of each field |
| `[ApiController]` automatic model-state check | Returns `400 Bad Request` with error details before the action runs |
| Custom `EmailExists()` check | Returns `409 Conflict` if email is already taken |

---

## GitHub Copilot Usage

Copilot was used throughout this project in the following ways:

1. **Writing boilerplate** – Copilot generated the initial controller scaffolding, saving setup time and avoiding typos in attribute names like `[ProducesResponseType]`.

2. **Debugging – duplicate email bug** – An early version of the `Update` endpoint would incorrectly flag the user's *own* email as a duplicate. Copilot suggested adding an `excludeId` parameter to `EmailExists()`, which fixed the logic.

3. **Middleware timing** – Copilot recommended using `Stopwatch` and logging at `Warning` level for slow requests (> 500 ms), which was added to `RequestLoggingMiddleware`.

4. **JWT configuration** – Copilot flagged that `ClockSkew` defaults to 5 minutes, allowing expired tokens to still pass. Setting `ClockSkew = TimeSpan.Zero` was a Copilot-driven fix.

5. **Security improvement** – Copilot warned that returning raw exception messages in production is a security risk, leading to the `GlobalExceptionMiddleware` that only exposes details in `Development` mode.
