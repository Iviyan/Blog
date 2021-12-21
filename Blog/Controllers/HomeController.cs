namespace Blog.Controllers;

[Route("/")]
public class HomeController : Controller
{

    [Route("")]
    public async Task<ViewResult> Index() => View();
}
