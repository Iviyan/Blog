using System.Security.Claims;
namespace Blog.Utils;

public static class AuthenticationManager
{
    public static async Task Authenticate(User user, HttpContext httpContext)
    {
        var claims = new List<Claim>
            {
                new(ClaimsIdentity.DefaultNameClaimType, user.Email),
                new("id", user.Id.ToString()),
                new("password_version", user.PasswordVersion.ToString())
            };

        ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
            ClaimsIdentity.DefaultRoleClaimType);

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
    }
}
