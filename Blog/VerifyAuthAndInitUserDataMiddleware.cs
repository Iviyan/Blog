using Blog.Utils;

namespace Blog;

public class VerifyAuthAndInitUserDataMiddleware
{
    RequestDelegate next;

    public VerifyAuthAndInitUserDataMiddleware(RequestDelegate next) =>
        this.next = next;

    public async Task InvokeAsync(HttpContext httpContext, ApplicationContext dbContext, IScopedData scopedData)
    {
        if (httpContext.User.Identity?.IsAuthenticated is true)
        {
            int id = int.Parse(httpContext.User.FindFirst("id")!.Value);
            short claimPasswordVersion = short.Parse(httpContext.User.FindFirst("password_version")!.Value);

            scopedData.AuthUserInfo = await dbContext.Users.Where(u => u.Id == id).Select(u => new AuthUserInfo
            {
                Id = u.Id,
                Login = u.Login,
                Email = u.Email,
                PasswordVersion = u.PasswordVersion,
                AvatarPath = Strings.Files.GetAvatarPathOrDefault(u.AvatarFileName),
                FullName = u.FullName,
                AvatarExists = u.AvatarFileName != null
            }).FirstAsync();

            if (scopedData.AuthUserInfo.PasswordVersion != claimPasswordVersion)
            {
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                ActionContext actionContext = new() { HttpContext = httpContext };
                await new ChallengeResult(CookieAuthenticationDefaults.AuthenticationScheme)
                    .ExecuteResultAsync(actionContext);
                return;
            }

            httpContext.Items[Strings.Context.UserId] = id;
        }
        await next(httpContext);
    }
}
