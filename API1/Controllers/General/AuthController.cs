using API1.Filters;
using API1.Interface;
using API1.Model;
using API1.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API1.Controllers.General
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _userService = userService;
            _authService = authService;
        }

        // Method to store refresh token in cookies
        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,   // Prevent JavaScript access
                Secure = true,     // Use Secure cookie for HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7) // Expire in 7 days
            };

            Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);
        }
        private void SetAccessTokenCookie(string accessToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,   // Prevent JavaScript access
                Secure = true,     // Use Secure cookie for HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7) // Expire in 7 days
            };

            Response.Cookies.Append("AccessToken", accessToken, cookieOptions);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var authTokenInstance = _authService.Login(request);
            if (authTokenInstance == null)
                return Unauthorized(new { message = "Invalid credentials" });

            // Store only the refresh token in cookies
            SetRefreshTokenCookie(authTokenInstance.RefreshToken);
            SetAccessTokenCookie(authTokenInstance.AccessToken);

            return Ok(new { message = "Login successful" });
        }

        //[HttpGet("login3")]  // Change to POST
        //public IActionResult Login3()
        //{
        //    var request = new LoginRequest
        //    {
        //        Username = "admin",
        //        Password = "admin123"
        //    };

        //    var authTokenInstance = _authService.Login(request);

        //    if (authTokenInstance == null)
        //    {
        //        return Unauthorized(new { message = "Invalid credentials" });
        //    }

        //    // Store only the refresh token in cookies
        //    SetRefreshTokenCookie(authTokenInstance.RefreshToken);

        //    return Redirect("/profile");
        //}

        [HttpGet("login2")]
        public IActionResult Login2()
        {
            var request = new LoginRequest
            {
                Username = "admin",
                Password = "admin123"
            };

            var authTokenInstance = _authService.Login(request);

            if (authTokenInstance == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }


            // Store the Access and Refresh token in cookies
            //SetRefreshTokenCookie(authTokenInstance.RefreshToken);
            //SetAccessTokenCookie(authTokenInstance.AccessToken);

            // Redirect to the React frontend profile page

            //return Redirect("http://localhost:3000/profile");
            //Response: http://localhost:3000/login redirect to login 

            //return Ok(Redirect("http://localhost:3000/profile"));
            //Response : { "permanent":false,"preserveMethod":false,"url":"http://localhost:3000/profile","urlHelper":null}

            //return Ok(new { redirectUrl = "http://localhost:3000/profile" });
            //Response: { "redirectUrl":"http://localhost:3000/profile"}

            // return new RedirectResult("http://localhost:3000/profile");
            //Response: http://localhost:3000/login  redirect to login 

            var reqToken = _authService.GetRedirectToken(request.Username);

            return new RedirectResult($"http://localhost:3000/redirectLogin?rdtkn={reqToken}");

        }


        [HttpGet("redirectloginUser")]
        [Authorize]
        public IActionResult RedirectloginUser()
        {
            var request = new LoginRequest
            {
                Username = "admin",
                Password = "admin123"
            };

            var authTokenInstance = _authService.Login(request);

            if (authTokenInstance == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }


            // Store the Access and Refresh token in cookies
            SetRefreshTokenCookie(authTokenInstance.RefreshToken);
            SetAccessTokenCookie(authTokenInstance.AccessToken);

            return Ok(new { redirectUrl = "http://localhost:3000/profile" });
        }


        [HttpPost("refresh")]
        public IActionResult RefreshToken()
        {
            // Retrieve refresh token from cookie
            var refreshToken = Request.Cookies["RefreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "No refresh token in cookies" });

            var newTokenInstance = _authService.RefreshToken(refreshToken);
            if (newTokenInstance == null)
                return Unauthorized(new { message = "Invalid refresh token" });

            // Store the new refresh token in cookies
            SetRefreshTokenCookie(newTokenInstance.RefreshToken);

            return Ok(new
            {
                message = "Token refreshed",
                accessToken = newTokenInstance.AccessToken
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("RefreshToken");
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("mydetails")]
        [Authorize]
        public IActionResult MyDetails()
        {
            var accessToken = Request.Cookies["AccessToken"];
            if (string.IsNullOrEmpty(accessToken))
                return Unauthorized(new { message = "No access token found!" });

            var username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { message = "No user logged in!" });

            var user = _userService.GetUserByUsername(username);
            return Ok(new { user });
        }
    }
}