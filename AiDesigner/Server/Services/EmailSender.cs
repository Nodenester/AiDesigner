using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebPWrecover.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger _logger;
        private readonly SmtpSettings _smtpSettings;

        public EmailSender(IOptions<SmtpSettings> smtpSettings, ILogger<EmailSender> logger)
        {
            _smtpSettings = smtpSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            using (var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = _smtpSettings.UseSSL
            })
            using (var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            })
            {
                mailMessage.To.Add(toEmail);
                try
                {
                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation($"Email to {toEmail} sent successfully!");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to send email to {toEmail}. Exception: {ex.Message}");
                    // Consider handling the failure further (e.g., retry logic, notification...)
                }
            }
        }
    }

    public class SmtpSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
    }
}




//using Microsoft.AspNetCore.Identity.UI.Services;
//using Microsoft.Extensions.Options;
//using SendGrid;
//using SendGrid.Helpers.Mail;

//namespace WebPWrecover.Services;

//public class EmailSender : IEmailSender
//{
//    private readonly ILogger _logger;

//    public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
//                       ILogger<EmailSender> logger)
//    {
//        Options = optionsAccessor.Value;
//        _logger = logger;
//    }

//    public AuthMessageSenderOptions Options { get; } //Set with Secret Manager.

//    public async Task SendEmailAsync(string toEmail, string subject, string message)
//    {
//        if (string.IsNullOrEmpty(Options.SendGridKey))
//        {
//            throw new Exception("Null SendGridKey");
//        }
//        await Execute(Options.SendGridKey, subject, message, toEmail);
//    }

//    public async Task Execute(string apiKey, string subject, string message, string toEmail)
//    {
//        var client = new SendGridClient(apiKey);
//        var msg = new SendGridMessage()
//        {
//            From = new EmailAddress("Joe@contoso.com", "Password Recovery"),
//            Subject = subject,
//            PlainTextContent = message,
//            HtmlContent = message
//        };
//        msg.AddTo(new EmailAddress(toEmail));

//        // Disable click tracking.
//        // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
//        msg.SetClickTracking(false, false);
//        var response = await client.SendEmailAsync(msg);
//        _logger.LogInformation(response.IsSuccessStatusCode
//                               ? $"Email to {toEmail} queued successfully!"
//                               : $"Failure Email to {toEmail}");
//    }
//}