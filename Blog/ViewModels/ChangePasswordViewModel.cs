using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; } = default!;

        [Required]
        [DataType(DataType.Password), StringLength(30, MinimumLength = 1)]
        public string NewPassword { get; set; } = default!;

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; } = default!;
    }
}