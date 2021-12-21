using Blog.Utils;

namespace Blog.Controllers;

public class ImagesController : Controller
{
    private static readonly string[] AllowedImageFileExtensions = { ".jpg", ".png" };
    private static int maxImageSize = 1024 * 1024;

    // TODO: Работоспособность не проверена
    [HttpPost("[controller]/upload")]
    public async Task<JsonResult> Upload(
        [FromForm(Name = "upload")] IFormFile file,
        [FromServices] IWebHostEnvironment env,
        [FromServices] IScopedData scopedData)
    {
        JsonResult ErrorResult(string msg) => new(new { Error = new { Message = msg } });

        AuthUserInfo? user = scopedData.AuthUserInfo;
        if (user is null)
            return ErrorResult("Загружать изображения могут только авторизованные пользователи.");

        if (file is { Length: > 0 })
        {
            if (file.Length > maxImageSize)
                return ErrorResult("Файл должен быть меньше 1Mb");

            var fileExt = Path.GetExtension(file.FileName);
            if (String.IsNullOrEmpty(fileExt) || !AllowedImageFileExtensions.Contains(fileExt))
                return ErrorResult("Допускаются лишь jpg и png файлы");

            string fileName = $"{user.Id}_{DateTime.Now:yyyyMMddHHmmssfff}{fileExt}";
            using FileStream fileStream = new($"{env.WebRootPath}{Strings.Files.GetUploadImagePath(fileName)}", FileMode.Create);
            await file.CopyToAsync(fileStream);

            return new JsonResult(new { Url = Strings.Files.GetUploadImagePath(fileName) });
        }

        return ErrorResult("Ошибка получения файла");
    }
}
