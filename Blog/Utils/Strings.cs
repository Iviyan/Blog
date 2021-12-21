namespace Blog.Utils;

public static class Strings
{
    public static class Context
    {
        public const string UserId = "UserId";
        public const string AvatarFileName = "AvatarFileName";
        public const string AvatarExists = "AvatarExists";
    }

    public static class Files
    {
        public const string AvatarPath = "/img/avatars/";
        public const string DefaultAvatar = "default.png";
        public const string DefaultAvatarPath = AvatarPath + DefaultAvatar;

        public static string GetAvatarPathOrDefault(string? fileName) => fileName == null ? DefaultAvatarPath : $"{AvatarPath}{fileName}";
        public static string? GetAvatarPath(string? fileName) => fileName == null ? null : $"{AvatarPath}{fileName}";
        
        public const string UploadImagePath = "/img/u/";

        public static string GetUploadImagePath(string fileName) => $"{UploadImagePath}{fileName}";
    }
}
