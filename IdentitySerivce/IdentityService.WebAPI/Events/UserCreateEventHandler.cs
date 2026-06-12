using IdentitySerivce.Domain;
using MyEventController;

namespace IdentityService.WebAPI.Events;

[EventName(IntegrationEventNames.IdentityAdminUserCreated)]
public class UserCreateEventHandler : JsonIntegrationEventHandler<UserCreateEvent>
{
    private readonly IEmailSender emailSender;
    
    public UserCreateEventHandler(IEmailSender emailSender)
    {
        this.emailSender = emailSender;
    }

    public override Task HandleJson(string eventName, UserCreateEvent? eventData)
    {
        return emailSender.SendEmailAsync(eventData?.Email ?? "", "账号创建成功", $"您的初始密码是{eventData.Password}");
    }
}