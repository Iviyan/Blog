namespace Blog.Models;

public class AuthUserInfo
{
    public int Id { get; set; }
    public string? Login { get; set; }
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public short PasswordVersion { get; set; }
    public string? AvatarPath { get; set; }
    public bool AvatarExists { get; set; }
}
