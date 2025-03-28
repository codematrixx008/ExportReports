using API1.Model;

namespace API1.Interface
{
    public interface IUserService
    {
        User? GetUserByUsername(string username);
        Task<List<string>> GetUserRolesAsync(string username);
        (bool IsValid, User? user) Authenticate(string username, string password);
        Task<bool> HasControllerAccessAsync(List<string> roles, string? controller, string? action);
    }
}
