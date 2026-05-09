namespace IdentitySerivce.Infrastructure.Options;

/// <summary>
/// 这是SendCloud邮件服务的配置类，存放发邮件需要的账号密码。
/// </summary>
public class CloudEmailSettings
{
    public string ApiUser { get; set; }
    public string ApiKey { get; set; }
    public string From { get; set; }
}