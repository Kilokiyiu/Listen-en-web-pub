using IdentitySerivce.Domain;
using MyEventController;

namespace IdentityService.WebAPI.Events;

[EventName("IdentityService.User.PasswordReset")]
public class ResetPwdEventHandler : JsonIntegrationEventHandler<ResetPwdEvent>
{
    private readonly ILogger<ResetPwdEventHandler> logger;
    private readonly IEmailSender emailSender;

    public ResetPwdEventHandler(ILogger<ResetPwdEventHandler> logger, IEmailSender emailSender)
    {
        this.logger = logger;
        this.emailSender = emailSender;
    }

    public override Task HandleJson(string eventname, ResetPwdEvent? eventData)
    {
        return emailSender.SendEmailAsync(eventData.Email, "密码重置通知", $"您的新密码是{eventData.Password}");
    }
}