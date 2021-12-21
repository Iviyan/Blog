using Blog.Utils;

namespace Blog.Controllers;

public class UserController : Controller
{
    [Route("/u/{idOrLogin}")]
    public async Task<ActionResult> Index(string idOrLogin, [FromServices] ApplicationContext context)
    {
        User? user = null;
        if (int.TryParse(idOrLogin, out int id))
            user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);
        else if (idOrLogin.Length > 0 && idOrLogin[0] is >= 'a' and <= 'z' or >= 'A' and <= 'Z')
            user = await context.Users.FirstOrDefaultAsync(u => u.Login == idOrLogin);

        if (user == null) return NotFound();

        return View(user);
    }

    [Route("/u/{idOrLogin}/info")]
    public async Task<ActionResult> Info(string idOrLogin, [FromServices] ApplicationContext context)
    {
        IQueryable<User>? query = null;
        if (int.TryParse(idOrLogin, out int id))
            query = context.Users.Where(x => x.Id == id);
        else if (idOrLogin.Length > 0 && idOrLogin[0] is >= 'a' and <= 'z' or >= 'A' and <= 'Z')
            query = context.Users.Where(u => u.Login == idOrLogin);

        if (query == null) return BadRequest();

        var user = await query
            .Select(u => new
            {
                u.Id,
                u.Login,
                u.FullName,
                u.PostCount,
                u.FollowerCount,
                Avatar = Strings.Files.GetAvatarPathOrDefault(u.AvatarFileName),
                u.AboutMe
            })
            .FirstOrDefaultAsync();

        if (user == null) return NotFound();

        return new ObjectResult(user);
    }


}
