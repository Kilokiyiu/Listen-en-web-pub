namespace IdentitySerivce.Infrastructure.Options;

/// <summary>
/// SMTP 发信配置（可用 Outlook / QQ 等邮箱直接发反馈邮件）。
/// </summary>
public class SmtpEmailSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string FromName { get; set; } = "ListenEase";
}
