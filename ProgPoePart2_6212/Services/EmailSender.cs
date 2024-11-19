using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ProgPoePart2_6212.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;

        public EmailSender()
        {
            // Replace these with your SMTP configuration settings.
            _smtpServer = "smtp.example.com";      // Your SMTP server address
            _smtpPort = 587;                      // Port number (587 for TLS, 465 for SSL)
            _smtpUser = "your-email@example.com"; // Your email address
            _smtpPass = "your-email-password";    // Your email password
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            using (var client = new SmtpClient(_smtpServer, _smtpPort))
            {
                client.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                client.EnableSsl = true; // Enables secure email sending (TLS/SSL)

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUser),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true // Set to true if the message contains HTML content
                };

                mailMessage.To.Add(email);

                try
                {
                    await client.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    // Handle any exceptions (e.g., log errors)
                    throw new InvalidOperationException("Failed to send email.", ex);
                }
            }
        }
    }
}
