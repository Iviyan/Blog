namespace Blog.Models
{
    public interface IScopedData
    {
        AuthUserInfo? AuthUserInfo { get; set; }
    }

    public class ScopedData : IScopedData
    {
        public AuthUserInfo? AuthUserInfo { get; set; }
    }
}
