using Blog.Utils;

namespace Blog.Controllers;

// TODO: оптимизировать запросы
[Authorize]
public class SettingsController : Controller
{
    User GetUser(ApplicationContext db) => db.Users.First(u => u.Id == authUserInfo.Id);

    AuthUserInfo authUserInfo;

    public SettingsController(IScopedData scopedData)
    {
        this.authUserInfo = scopedData.AuthUserInfo!;
    }

    [Route("/[controller]/{page?}")]
    public IActionResult Settings([FromServices] ApplicationContext db, string page)
    {
        if (page is null or not ("Info" or "Security"))
            return RedirectToAction("Settings", new { page = "Info" });

        User user = GetUser(db);
        return View(new SettingsModel { User = user });
    }

    [HttpPost("[controller]/Info")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeInfo([FromServices] ApplicationContext db,
        ChangeUserInfoViewModel model)
    {
        User user = GetUser(db);

        if (ModelState.IsValid)
        {
            if (model.AboutMe?.Contains("<script ", StringComparison.OrdinalIgnoreCase) is true)
                ModelState.AddModelError(nameof(model.AboutMe), "Поле не может содержать тег script.");
            else
            if (!await db.Users.AnyAsync(u => u.Login == model.Login && u.Id != user.Id))
            {
                user.Login = model.Login;
                user.FullName = model.FullName;
                user.AboutMe = model.AboutMe;
                await db.SaveChangesAsync();
            }
            else
                ModelState.AddModelError(nameof(model.Login), "Пользователь с таким логином уже существует.");
        }

        return View("Settings", new SettingsModel { User = user, UserInfoViewModel = model });
    }

    [HttpPost("[controller]/Security")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword([FromServices] ApplicationContext db,
        ChangePasswordViewModel model)
    {
        User user = GetUser(db);

        if (ModelState.IsValid)
        {
            if (user.Password == model.OldPassword)
            {
                user.Password = model.NewPassword;
                await db.SaveChangesAsync();

                user.PasswordVersion = await db.Users.Where(u => u.Id == user.Id).Select(u => u.PasswordVersion).FirstAsync();

                await AuthenticationManager.Authenticate(user, HttpContext);
            }
            else
                ModelState.AddModelError(nameof(model.OldPassword), "Введённый пароль не совпадает с текущим");
        }

        return View("Settings", new SettingsModel { User = user, PasswordViewModel = model });
    }

    [HttpPost]
    public async Task<IActionResult> ResetAvatar(
        [FromServices] DatabaseConnectionStrings connectionStrings,
        [FromServices] IWebHostEnvironment env)
    {
        if (authUserInfo.AvatarExists)
        {
            using var con = new NpgsqlConnection(connectionStrings.Pgsql);
            await con.OpenAsync();
            await using var cmd =
                new NpgsqlCommand($"update users set avatar_file_name = null where id = {authUserInfo.Id}", con);
            cmd.ExecuteNonQuery();
            System.IO.File.Delete(
                $"{env.WebRootPath}{authUserInfo.AvatarPath}");
        }

        return RedirectToAction("Settings");
    }

    private static readonly string[] AllowedImageFileExtensions = { ".jpg", ".png" };
    private static int maxImageSize = 1024 * 1024; // 1Mb

    [HttpPost("[controller]/UploadAvatar")]
    public async Task<JsonResult> UploadAvatar(
        [FromForm(Name = "file")] IFormFile file,
        [FromServices] DatabaseConnectionStrings connectionStrings,
        [FromServices] IWebHostEnvironment env)
    {
        if (file is { Length: > 0 })
        {
            if (file.Length > maxImageSize)
                return new JsonResult(new { Error = "Файл должен быть меньше 1Mb" });

            var fileExt = Path.GetExtension(file.FileName);
            if (String.IsNullOrEmpty(fileExt) || !AllowedImageFileExtensions.Contains(fileExt))
                return new JsonResult(new { Error = "Допускаются лишь jpg и png файлы" });

            string fileName = authUserInfo.Id + fileExt;
            using FileStream fileStream = new($"{env.WebRootPath}{Strings.Files.GetAvatarPath(fileName)}", FileMode.Create);
            await file.CopyToAsync(fileStream);

            await using var con = new NpgsqlConnection(connectionStrings.Pgsql);
            await con.OpenAsync();
            await using var cmd =
                new NpgsqlCommand($"update users set avatar_file_name = '{fileName}' where id = {authUserInfo.Id}", con);
            cmd.ExecuteNonQuery();

            return new JsonResult(new { Error = 0, Url = Strings.Files.GetAvatarPath(fileName) });
        }

        return new JsonResult(new { Error = "Ошибка получения файла" });
    }
}
