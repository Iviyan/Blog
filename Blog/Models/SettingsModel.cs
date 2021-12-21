using Blog.ViewModels;

namespace Blog.Models
{
    public class SettingsModel
    {
        public User User { get; set; } = default!;
        public ChangeUserInfoViewModel? UserInfoViewModel { get; set; }
        public ChangePasswordViewModel? PasswordViewModel { get; set; }

        public static implicit operator SettingsModel(User user) => new() { User = user };
    }
}