using IdentitySerivce.Domain;
using MyEventController;

namespace IdentityService.WebAPI.Events;

[EventName("IdentityService.User.CreateUserEventHandler")]
public class CreateUserEventHandler : JsonIntegrationEventHandler<CreatUserEvent>
{
    private readonly ILogger<CreateUserEventHandler> logger;
    private readonly IEmailSender emailSender;

    public CreateUserEventHandler(ILogger<CreateUserEventHandler> logger, IEmailSender emailSender)
    {
        this.logger = logger;
        this.emailSender = emailSender;
    }

    public override Task HandleJson(string eventname, CreatUserEvent? eventData)
    {
        return emailSender.SendEmailAsync(eventData.Email, "账号创建成功",
            $"您的新账号为：{eventData.Password},密码{eventData.Password}");
    }

}