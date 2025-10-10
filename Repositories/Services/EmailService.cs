using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Threading.Tasks;

namespace Repositories.Services
{
    public class EmailService
    {
        // You can move these to config or environment variables
        private const string FromName = "BCAS Psychonnect System";
        private const string FromEmail = "fromAddress@gmail.com";
        private const string SmtpHost = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SmtpUser = "brentjaydeleon@gmail.com";
        private const string SmtpPass = "tmet gker epqp eglt"; // Better store in config!

        //lance
        //private const string SmtpUser = "devillalancechristian1@gmail.com";
        //private const string SmtpPass = "ciko gtyb oodb clpy";

        public async Task SendEmailAsync(string email, string subject, string messageBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(FromName, FromEmail));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = messageBody
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(SmtpHost, SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(SmtpUser, SmtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
