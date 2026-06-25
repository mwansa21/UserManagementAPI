using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers
{
    /// <summary>
    /// Issues JWT tokens for demo authentication.
    /// In production, validate credentials against a real identity store.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        // Demo credentials – in production these come from a secure identity store.
        private static readonly Dictionary<string, string> _demoUsers = new(StringComparer.OrdinalIgnoreCase)
        {
            { "admin", "Admin@123" },
            { "user",  "User@123"  }
        };

        public AuthController(IConfiguration config, ILogger<AuthController> logger)
        {
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate with username + password and receive a JWT Bearer token.
        /// Demo credentials:  admin / Admin@123  or  user / User@123
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (!_demoUsers.TryGetValue(request.Username, out var expectedPassword)
                || expectedPassword != request.Password)
            {
                _logger.LogWarning("Failed login attempt for username: {Username}", request.Username);
                return Unauthorized(new { message = "Invalid username or password." });
            }

            var token = GenerateJwtToken(request.Username);
            _logger.LogInformation("User '{Username}' authenticated successfully.", request.Username);

            return Ok(new
            {
                token,
                expiresIn = 3600,
                tokenType = "Bearer"
            });
        }

        // ── Private helpers ──────────────────────────────────────────────────

        private string GenerateJwtToken(string username)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var creds       = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name,               username),
                new Claim(ClaimTypes.Role,               username.Equals("admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User"),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iss,   jwtSettings["Issuer"]!),
                new Claim(JwtRegisteredClaimNames.Aud,   jwtSettings["Audience"]!)
            };

            var token = new JwtSecurityToken(
                issuer:             jwtSettings["Issuer"],
                audience:           jwtSettings["Audience"],
                claims:             claims,
                expires:            DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
