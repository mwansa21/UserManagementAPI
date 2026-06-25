# User Management API

A **.NET 8 Web API** for managing users, built as part of the *Building a Simple API with Copilot* assignment.



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




