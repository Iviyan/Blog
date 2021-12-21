namespace Blog.ViewModels;

public class LoginViewModel
{
    [Required] public string Email { get; set; } = default!;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = default!;
}
