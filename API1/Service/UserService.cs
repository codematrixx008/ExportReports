using API1.Interface;
using API1.Model;
using System.Collections.Generic;
using System.Linq;

namespace API1.Service
{
    public class UserService : IUserService
    {
        // Mock user data
        public static readonly List<User> _users = new()
        {
            new User { Id = 1, Username = "admin", Password = "admin123", Roles = new List<string> { "Admin"} },
            new User { Id = 2, Username = "user", Password = "user123", Roles = new List<string> { "User", "Manager" } }
        };

        public static readonly Dictionary<string, List<string>> _accessRules = new()
        {
            { "Admin", new List<string> { "Admin" } },
            { "Reports", new List<string> { "Manager", "Manager","Admin" } },
            { "User", new List<string> { "Admin", "User" } }
        };

        public User? GetUserByUsername(string username)
        {
            return _users.FirstOrDefault(u => u.Username == username);
        }

        public async Task<List<string>> GetUserRolesAsync(string username)
        {
            var user = GetUserByUsername(username);
            return await Task.FromResult(user?.Roles ?? new List<string>());
        }

        public (bool IsValid, User? user) Authenticate(string username, string password)
        {
            var user = _users.SingleOrDefault(u => u.Username == username && u.Password == password);
            return user != null ? (true, user) : (false, null);
        }

        public Task<bool> HasControllerAccessAsync(List<string> roles, string? controller, string? action)
        {
            if (controller != null && _accessRules.ContainsKey(controller))
            {
                return Task.FromResult(roles.Any(role => _accessRules[controller].Contains(role)));
            }
            return Task.FromResult(false);
        }
    }

}
