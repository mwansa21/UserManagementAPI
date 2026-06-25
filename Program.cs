using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UserManagementAPI.Data;
using UserManagementAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── Services ─────────────────────────────────────────────────────────────────

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Register the in-memory user store as a singleton so data persists
// across requests within a single run of the app.
builder.Services.AddSingleton<UserStore>();

// ── JWT Authentication ────────────────────────────────────────────────────────
// Copilot suggestion: use IOptions pattern + AddAuthentication extension to keep
// Program.cs clean and avoid duplicating key-reading logic.
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey   = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSettings["Issuer"],
        ValidAudience            = jwtSettings["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew                = TimeSpan.Zero   // no grace period on expiry
    };
});

builder.Services.AddAuthorization();

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
// Configured to accept a Bearer token so you can test protected endpoints
// directly from the Swagger UI.
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "User Management API",
        Version     = "v1",
        Description = "A .NET 8 REST API for managing users, built with GitHub Copilot."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token. Example: Bearer eyJhbGci..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── Build ─────────────────────────────────────────────────────────────────────

var app = builder.Build();

// ── Middleware pipeline (order matters) ───────────────────────────────────────

// 1. Global exception handler – must be first so it wraps everything else.
app.UseGlobalExceptionHandling();

// 2. Request / response logging.
app.UseRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API v1");
        c.RoutePrefix = string.Empty; // serve Swagger UI at the root URL
    });
}

app.UseHttpsRedirection();

// 3. Authentication before Authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
