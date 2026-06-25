using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Data;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers
{
    /// <summary>
    /// CRUD endpoints for managing users.
    /// All write operations require a valid JWT (Bearer token).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly UserStore _store;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserStore store, ILogger<UsersController> logger)
        {
            _store  = store;
            _logger = logger;
        }

        // ─── GET /api/users ────────────────────────────────────────────────
        /// <summary>Retrieve all active users.</summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var users = _store.GetAll();
            _logger.LogInformation("Retrieved {Count} users.", users.Count());
            return Ok(users);
        }

        // ─── GET /api/users/{id} ───────────────────────────────────────────
        /// <summary>Retrieve a single user by ID.</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById(int id)
        {
            var user = _store.GetById(id);
            if (user is null)
            {
                _logger.LogWarning("User {Id} not found.", id);
                return NotFound(new { message = $"User with ID {id} was not found." });
            }
            return Ok(user);
        }

        // ─── POST /api/users ───────────────────────────────────────────────
        /// <summary>Create a new user. Requires authentication.</summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Create([FromBody] UserDto dto)
        {
            // ModelState validation is handled automatically by [ApiController]
            // but we add a custom email-uniqueness check here.
            if (_store.EmailExists(dto.Email))
            {
                _logger.LogWarning("Duplicate email attempt: {Email}", dto.Email);
                return Conflict(new { message = "A user with that email already exists." });
            }

            var user = new User
            {
                Name        = dto.Name,
                Email       = dto.Email,
                Role        = dto.Role,
                PhoneNumber = dto.PhoneNumber
            };

            var created = _store.Add(user);
            _logger.LogInformation("Created user {Id} ({Email}).", created.Id, created.Email);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ─── PUT /api/users/{id} ───────────────────────────────────────────
        /// <summary>Update an existing user. Requires authentication.</summary>
        [HttpPut("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Update(int id, [FromBody] UserDto dto)
        {
            if (_store.GetById(id) is null)
                return NotFound(new { message = $"User with ID {id} was not found." });

            if (_store.EmailExists(dto.Email, excludeId: id))
                return Conflict(new { message = "Another user already owns that email address." });

            var updated = _store.Update(id, new User
            {
                Name        = dto.Name,
                Email       = dto.Email,
                Role        = dto.Role,
                PhoneNumber = dto.PhoneNumber
            });

            _logger.LogInformation("Updated user {Id}.", id);
            return Ok(updated);
        }

        // ─── DELETE /api/users/{id} ────────────────────────────────────────
        /// <summary>Soft-delete a user. Requires authentication.</summary>
        [HttpDelete("{id:int}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            if (!_store.Delete(id))
            {
                _logger.LogWarning("Delete failed – user {Id} not found.", id);
                return NotFound(new { message = $"User with ID {id} was not found." });
            }

            _logger.LogInformation("Soft-deleted user {Id}.", id);
            return NoContent();
        }
    }
}
