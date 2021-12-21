namespace Blog.Models;

[Table("users")]
public class User
{
    [Column("id"), Key] public int Id { get; set; }
    [Column("login")] public string? Login { get; set; }
    [Column("email")] public string Email { get; set; } = default!;
    [Column("password")] public string Password { get; set; } = default!;
    [Column("full_name")] public string FullName { get; set; } = default!;
    [Column("about_me")] public string? AboutMe { get; set; } = default!;
    [Column("password_version"), DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public short PasswordVersion { get; set; }
    [Column("avatar_file_name")] public string? AvatarFileName { get; set; }
    [Column("post_count"), DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public int PostCount { get; set; }
    [Column("follower_count"), DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public int FollowerCount { get; set; }
}
