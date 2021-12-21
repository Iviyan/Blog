using Blog.Utils;

namespace Blog.Controllers;

[Route("api")]
[ApiController]
public class ApiController : ControllerBase
{
    ApplicationContext context;
    AuthUserInfo? authUserInfo;

    public ApiController(ApplicationContext context, IScopedData scopedData)
    {
        this.context = context;
        this.authUserInfo = scopedData.AuthUserInfo;
    }

    [HttpGet("posts")]
    public async Task<ActionResult> GetPosts([Required]int userId, int? count, int? offset, int? lastId)
    {
        if (offset is { } && lastId is { })
            ModelState.AddModelError("", "Нужно использовать либо offset либо lastId");
        if ((count ??= 1) > 100)
            ModelState.AddModelError(nameof(count), "count не может превышать 100");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);


        IQueryable<Post> query = lastId is { }
            ? context.Posts.Where(p => p.UserId == userId && p.Id < lastId)
            : context.Posts.Where(p => p.UserId == userId);

        var posts = await query.OrderByDescending(p => p.Id).Skip(offset ?? 0).Take(count.Value).ToListAsync();

        return new ObjectResult(new
        {
            Count = await context.Users.Where(u => u.Id == userId).Select(u => u.PostCount).FirstAsync(),
            Items = posts,
            Users = context.Users
                .Where(u => posts.Select(p => p.UserId).Distinct().Contains(u.Id))
                .Select(u => new { u.Id, u.Login, u.FullName, Avatar = Strings.Files.GetAvatarPathOrDefault(u.AvatarFileName) })
        });
    }

    [HttpGet("comments")]
    public async Task<ActionResult<IEnumerable<Comment>>> GetComments([Required] int postId, int? count, int? offset, int? lastId)
    {
        if (offset is { } && lastId is { })
            ModelState.AddModelError("", "Нужно использовать либо offset либо lastId");
        if ((count ??= 1) > 100)
            ModelState.AddModelError(nameof(count), "count не может превышать 100");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);


        IQueryable<Comment> query = lastId is { }
            ? context.Comments.Where(p => p.PostId == postId && p.Id > lastId)
            : context.Comments.Where(p => p.PostId == postId);

        var comments = await query.OrderBy(c => c.Id).Skip(offset ?? 0).Take(count.Value).ToListAsync();

        return new ObjectResult(new
        {
            Count = await context.Posts.Where(p => p.Id == postId).Select(p => p.CommentCount).FirstAsync(),
            Items = comments,
            Users = context.Users
                .Where(u => comments.Select(c => c.UserId).Distinct().Contains(u.Id))
                .Select(u => new { u.Id, u.Login, u.FullName, Avatar = Strings.Files.GetAvatarPathOrDefault(u.AvatarFileName) })
        });
    }

    public record PostModel([Required, StringLength(10000)] string Text);

    [HttpPost("posts"), Authorize]
    public async Task<ActionResult> AddPost(PostModel data)
    {
        Post post = new Post { UserId = authUserInfo!.Id, Body = data.Text };
        await context.Posts.AddAsync(post);
        await context.SaveChangesAsync();

        return new ObjectResult(post);
    }

    [HttpDelete("posts/{id}"), Authorize]
    public async Task<ActionResult> DeletePost(int id)
    {
        Post? post = await context.Posts.Where(c => c.Id == id).Select(c => new Post { Id = c.Id }).FirstOrDefaultAsync();
        if (post is null)
            return NotFound();

        context.Posts.Remove(post);
        await context.SaveChangesAsync();

        return Ok();
    }

    public record AddCommentModel([Required] int PostId, [Required, StringLength(1000)] string Text);

    [HttpPost("comments"), Authorize]
    public async Task<ActionResult> AddComment(AddCommentModel data)
    {
        Comment comment = new Comment { UserId = authUserInfo!.Id, PostId = data.PostId, Body = data.Text };
        await context.Comments.AddAsync(comment);
        await context.SaveChangesAsync();

        return new ObjectResult(comment);
    }

    [HttpDelete("comments/{id}"), Authorize]
    public async Task<ActionResult> DeleteComment(int id)
    {
        Comment? comment = await context.Comments.Where(c => c.Id == id).Select(c => new Comment { Id = c.Id }).FirstOrDefaultAsync();
        if (comment is null)
            return NotFound();

        context.Comments.Remove(comment);
        await context.SaveChangesAsync();

        return Ok();
    }

    private bool PostExists(long id) => context.Posts.Any(e => e.Id == id);

    [HttpPut("posts/{id}"), Authorize]
    public async Task<IActionResult> UpdatePost(int id, PostModel data)
    {
        Post post = new Post { Id = id, Body = data.Text };

        context.Entry(post).Property(p => p.Body).IsModified = true;

        try
        {
            await context.SaveChangesAsync();
            return Ok();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PostExists(id))
                return NotFound();
            else
                throw;
        }
    }

    private bool CommentExists(long id) => context.Comments.Any(e => e.Id == id);

    public record CommentModel([Required, StringLength(1000)] string Text);

    [HttpPut("comments/{id}"), Authorize]
    public async Task<IActionResult> UpdateComment(int id, CommentModel data)
    {
        Comment comment = new Comment { Id = id, Body = data.Text };

        context.Entry(comment).Property(p => p.Body).IsModified = true;

        try
        {
            await context.SaveChangesAsync();
            return Ok();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PostExists(id))
                return NotFound();
            else
                throw;
        }
    }

    [HttpGet("mybaseinfo")]
    public ActionResult GetMyBaseInfo([FromServices] IScopedData scopedData)
    {
        var userInfo = scopedData.AuthUserInfo;

        if (userInfo == null)
            return Unauthorized();

        return new ObjectResult(new {
            userInfo.Id,
            userInfo.Login,
            userInfo.FullName,
            Avatar = userInfo.AvatarPath
        });
    }

    [Route("/id")]
    public ActionResult GetId() => new ObjectResult(new { Id = User.FindFirst("id")?.Value });
}