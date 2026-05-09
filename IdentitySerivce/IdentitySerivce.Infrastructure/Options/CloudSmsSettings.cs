namespace IdentitySerivce.Infrastructure.Options;

/// <summary>
/// 这是SendCloud通过断行发送消息的配置类，存放发送短信需要的账号密码
/// </summary>
public class CloudSmsSettings
{
    public string SmsUser { get; set; }
    public string SmsKey { get; set; }
}