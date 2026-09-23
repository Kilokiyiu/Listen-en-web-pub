using System.Net;
using System.Net.Mail;
using IdentitySerivce.Domain;
using IdentitySerivce.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentitySerivce.Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly ILogger<SmtpEmailSender> logger;
    private readonly SmtpEmailSettings settings;

    public SmtpEmailSender(ILogger<SmtpEmailSender> logger, IOptionsSnapshot<SmtpEmailSettings> settings)
    {
        this.logger = logger;
        this.settings = settings.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        if (string.IsNullOrWhiteSpace(settings.Host)
            || string.IsNullOrWhiteSpace(settings.UserName)
            || string.IsNullOrWhiteSpace(settings.Password))
        {
            throw new InvalidOperationException("SMTP 配置不完整，请检查 Host / UserName / Password");
        }

        var fromAddress = string.IsNullOrWhiteSpace(settings.From) ? settings.UserName : settings.From;
        logger.LogInformation("SMTP Email to {To}, title:{Subject}", toEmail, subject);

        using var client = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(settings.UserName, settings.Password),
            Timeout = 15000
        };

        using var mail = new MailMessage
        {
            From = new MailAddress(fromAddress, settings.FromName),
            Subject = subject,
            Body = message,
            IsBodyHtml = false
        };
        mail.To.Add(toEmail);

        try
        {
            await client.SendMailAsync(mail);
            logger.LogInformation("SMTP Email sent successfully to {To}", toEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SMTP Email failed to {To} via {Host}:{Port}", toEmail, settings.Host, settings.Port);
            throw;
        }
    }
}
