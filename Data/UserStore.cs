using UserManagementAPI.Models;

namespace UserManagementAPI.Data
{
    /// <summary>
    /// A simple in-memory store that acts as a stand-in for a real database.
    /// Seeded with sample users so the API works out of the box.
    /// </summary>
    public class UserStore
    {
        private static int _nextId = 4;
        private readonly List<User> _users = new()
        {
            new User { Id = 1, Name = "Alice Johnson",  Email = "alice@example.com",  Role = "Admin",     PhoneNumber = "555-0101", CreatedAt = DateTime.UtcNow.AddDays(-30) },
            new User { Id = 2, Name = "Bob Smith",      Email = "bob@example.com",    Role = "User",      PhoneNumber = "555-0102", CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new User { Id = 3, Name = "Carol Williams", Email = "carol@example.com",  Role = "Moderator", PhoneNumber = "555-0103", CreatedAt = DateTime.UtcNow.AddDays(-10) }
        };

        public IEnumerable<User> GetAll() => _users.Where(u => u.IsActive);

        public User? GetById(int id) => _users.FirstOrDefault(u => u.Id == id && u.IsActive);

        public bool EmailExists(string email, int? excludeId = null) =>
            _users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)
                         && u.IsActive
                         && u.Id != excludeId);

        public User Add(User user)
        {
            user.Id = _nextId++;
            user.CreatedAt = DateTime.UtcNow;
            _users.Add(user);
            return user;
        }

        public User? Update(int id, User updated)
        {
            var existing = GetById(id);
            if (existing is null) return null;

            existing.Name        = updated.Name;
            existing.Email       = updated.Email;
            existing.Role        = updated.Role;
            existing.PhoneNumber = updated.PhoneNumber;
            return existing;
        }

        public bool Delete(int id)
        {
            var user = GetById(id);
            if (user is null) return false;

            user.IsActive = false;   // soft delete
            return true;
        }
    }
}
