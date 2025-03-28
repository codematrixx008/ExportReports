using API1.Model;
using System.Security.Claims;

namespace API1.Interface
{
    public interface IJWTService
    {
        AccessTokens GenerateAccessToken(User user, bool blnGenerateRefreshToken);
        string GenerateRefreshToken();
        string GenerateRedirectToken(string userName);
    }
}
