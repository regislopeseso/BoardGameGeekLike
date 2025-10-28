using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MimeKit;
using SysTask = System.Threading.Tasks.Task;

namespace BoardGameGeekLike.Services
{
    public class MailtrapSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
    }

    public interface IEmailService
    {
        SysTask SendEmailAsync(string to, string subject, string htmlBody);
        SysTask SendPasswordResetEmailAsync(string to, string resetLink, string userName, string gender);
    }

    public class MailtrapEmailService : IEmailService
    {
        private readonly MailtrapSettings _settings;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public MailtrapEmailService(IOptions<MailtrapSettings> options, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _settings = options.Value;
            _environment = environment;
            _configuration = configuration;
        }

        public async SysTask SendEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    Console.WriteLine($"Connecting to Mailtrap SMTP: {_settings.Host}:{_settings.Port}");

                    await client.ConnectAsync(_settings.Host, _settings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_settings.Username, _settings.Password);

                    Console.WriteLine($"Sending email to: {to}");
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);

                    Console.WriteLine("Email sent successfully via Mailtrap!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mailtrap SMTP Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        public async SysTask SendPasswordResetEmailAsync(string to, string resetLink, string userName, string gender)
        {
            string subject = "Password Reset Request - BBGLIKE2.0";

            // Load template from file
            var templatePath = Path.Combine(_environment.ContentRootPath, "Templates", "PasswordResetEmail.html");
            var template = await File.ReadAllTextAsync(templatePath);

            // Get frontend URL from configuration
            var frontendUrl = _configuration["FrontendBaseUrl"] ?? "http://localhost:5173";

            // Replace placeholders
            var html = template
                .Replace("{{USERNAME}}", userName)
                .Replace("{{GENDER}}", gender)
                .Replace("{{RESET_LINK}}", resetLink)
                .Replace("{{FRONTEND_URL}}", frontendUrl);

            await SendEmailAsync(to, subject, html);
        }
    }
}