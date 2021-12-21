using Blog.Utils;

namespace Blog.Controllers;

public class AccountController : Controller
{
    [Route("/Login"), HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated is true)
            return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost("/Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromServices] ApplicationContext db, LoginViewModel viewModel,
        string? returnUrl = "")
    {
        if (ModelState.IsValid)
        {
            User? user = await db.Users.FirstOrDefaultAsync(u =>
                u.Email == viewModel.Email && u.Password == viewModel.Password);
            if (user != null)
            {
                await AuthenticationManager.Authenticate(user, HttpContext);

                if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Некорректные логин и(или) пароль");
        }

        return View(viewModel);
    }

    [Route("/Register"), HttpGet]
    public IActionResult Register() => View();

    [HttpPost("/Register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register([FromServices] ApplicationContext db, RegisterViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            if (!await db.Users.AnyAsync(u => u.Email == viewModel.Email))
            {
                User user = new()
                {
                    Email = viewModel.Email,
                    Password = viewModel.Password,
                    FullName = viewModel.FullName
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();

                await AuthenticationManager.Authenticate(user, HttpContext);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(nameof(viewModel.Email), "Пользователь с таким email уже существует.");
        }

        return View(viewModel);
    }

    [Route("/Logout"), Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
