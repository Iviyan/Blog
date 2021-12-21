namespace Blog.ViewModels;

public class RegisterViewModel
{
    [EmailAddress,
     StringLength(255),
     Required]
    public string Email { get; set; } = default!;

    [DataType(DataType.Password),
     StringLength(30, MinimumLength = 1),
     Required]
    public string Password { get; set; } = default!;

    [DataType(DataType.Password)]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = default!;

    [Required, StringLength(70)] public string FullName { get; set; } = default!;
}
