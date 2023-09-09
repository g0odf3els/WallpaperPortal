namespace WallpaperPortal
{
    public class ApplicationRoutes
    {
        public static class Authorizaton
        {
            private const string Base = nameof(Authorizaton);

            public const string Register = Base + "/" + nameof(Register);
            public const string Login = Base + "/" + nameof(Login);
            public const string Logout = Base + "/" + nameof(Logout);
        }
    }
}
