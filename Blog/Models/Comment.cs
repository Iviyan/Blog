namespace Blog.Models;

[Table("comments")]
public class Comment
{
    [Column("id"), Key] public int Id { get; set; }
    [Column("post_id")] public int PostId { get; set; }
    [Column("user_id")] public int UserId { get; set; }
    [Column("body")] public string Body { get; set; } = default!;
    [Column("creation_date"), DatabaseGenerated(DatabaseGeneratedOption.Computed)] 
    public DateTime CreationDate { get; set; }
}
