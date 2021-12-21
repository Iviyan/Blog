namespace Blog.ViewModels;

public class ChangeUserInfoViewModel
{
    [RegularExpression("^[a-zA-Z][a-zA-Z0-9_]+$")]
    [StringLength(20, MinimumLength = 3)]
    public string? Login { get; set; }

    [Required, StringLength(70)]
    public string FullName { get; set; } = default!;

    [StringLength(10000)]
    public string? AboutMe { get; set; }
}
