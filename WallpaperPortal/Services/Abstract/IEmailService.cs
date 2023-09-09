namespace WallpaperPortal.Services.Abstract
{
    public interface IEmailService
    {
        public Task SendEmailAsync(string email, string subject, string message);
    }
}
