using Microsoft.Extensions.Logging;

namespace IdentitySerivce.Infrastructure.Services;

/// <summary>
/// 在开发环境下使用，用于模拟邮件的发送
/// </summary>
public class MockEmailSender : IEmailSender
{
    private readonly ILogger<MockEmailSender> logger;

    public MockEmailSender(ILogger<MockEmailSender> logger)
    {
        this.logger = logger;
    }

    public Task SendEmailAsync(string toEmail, string subject, string message)
    {
        logger.LogInformation("Sending email to {0}, title{1}, message{3}", toEmail,  subject, message);
        return Task.CompletedTask;
    }
}