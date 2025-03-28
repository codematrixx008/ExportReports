using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API1.Interface;
using API1.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace API1.Service
{
    public class AuthService : IAuthService
    {
        private readonly IJWTService _jwtService;
        private readonly IUserService _userService;
        private readonly Dictionary<string, string> _refreshTokens = new(); // Stores Refresh Tokens

        public AuthService(IJWTService jwtService, IUserService userService)
        {
            _jwtService = jwtService;
            _userService = userService;
        }

        public AccessTokens Login(LoginRequest request)
        {
            var authUser = _userService.Authenticate(request.Username, request.Password);
            if (!authUser.IsValid)
                return null;

            var user = authUser.user;
            var tokenInstance = _jwtService.GenerateAccessToken(user,true);
            _refreshTokens[user.Username] = tokenInstance.RefreshToken;
            return tokenInstance;
        }

        public AccessTokens RefreshToken(string refreshToken)
        {
            foreach (var entry in _refreshTokens)
            {
                if (entry.Value == refreshToken)
                {
                    var user = _userService.GetUserByUsername(entry.Key);
                    if (user != null)
                    {
                        var tokenInstance = _jwtService.GenerateAccessToken(user,true);
                        return tokenInstance;
                    }
                }
            }
            return null;
        }


        public string GetRedirectToken(string userName)
        {
            string redirectToken = _jwtService.GenerateRedirectToken(userName);
            return redirectToken;
        }

    }
}
