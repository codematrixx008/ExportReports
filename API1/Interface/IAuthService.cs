using API1.Model;
using System.Security.Claims;

namespace API1.Interface
{
    public interface IAuthService
    {
        AccessTokens Login(LoginRequest request);
        AccessTokens RefreshToken(string refreshToken);
        string GetRedirectToken(string userName);
    }

}
