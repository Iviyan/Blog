namespace Blog.Models;

[Table("posts")]
public class Post
{
    [Column("id"), Key] public int Id { get; set; }
    [Column("user_id")] public int UserId { get; set; }
    [Column("body")] public string Body { get; set; } = default!;
    [Column("creation_date"), DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreationDate { get; set; }
    [Column("comment_count"), DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public int CommentCount { get; set; }
}