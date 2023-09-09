using MailKit.Net.Smtp;
using MimeKit;
using WallpaperPortal.Services.Abstract;

namespace WallpaperPortal.Services
{
    public class EmailService : IEmailService
    {
        private readonly string smtpHost;
        private readonly string smtpPort;
        private readonly string smtpUsername;
        private readonly string smtpPassword;

        public EmailService(IConfiguration configuration)
        {
            smtpHost = configuration["Smtp:Host"];
            smtpPort = configuration["Smtp:Port"];
            smtpUsername = configuration["Smtp:Username"];
            smtpPassword = configuration["Smtp:Password"];
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Site administration", "admin@metanit.com"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpHost, Int32.Parse(smtpPort), true);
                await client.AuthenticateAsync(smtpUsername, smtpPassword);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}
