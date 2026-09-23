using System.Security.Claims;
using System.Text;
using IdentitySerivce.Domain;
using IdentitySerivce.Infrastructure.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IdentityService.WebAPI.Controllers;

[ApiController]
[Route("/api/identity/[controller]/[action]")]
public class FeedbackController : ControllerBase
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly FeedbackSettings feedbackSettings;
    private readonly ILogger<FeedbackController> logger;

    public FeedbackController(
        IServiceScopeFactory scopeFactory,
        IOptionsSnapshot<FeedbackSettings> feedbackSettings,
        ILogger<FeedbackController> logger)
    {
        this.scopeFactory = scopeFactory;
        this.feedbackSettings = feedbackSettings.Value;
        this.logger = logger;
    }

    [HttpPost]
    [AllowAnonymous]
    public IActionResult Submit([FromBody] SubmitFeedbackRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("请填写反馈内容");
        }

        var message = request.Message.Trim();
        if (message.Length < 5)
        {
            return BadRequest("反馈内容至少 5 个字符");
        }

        if (message.Length > 2000)
        {
            return BadRequest("反馈内容不能超过 2000 个字符");
        }

        var category = string.IsNullOrWhiteSpace(request.Category) ? "其他" : request.Category.Trim();
        if (category.Length > 50)
        {
            return BadRequest("分类无效");
        }

        var name = request.Name?.Trim() ?? string.Empty;
        if (name.Length > 50)
        {
            return BadRequest("称呼过长");
        }

        var email = request.Email?.Trim() ?? string.Empty;
        if (email.Length > 100)
        {
            return BadRequest("邮箱过长");
        }

        if (!string.IsNullOrEmpty(email) && !email.Contains('@'))
        {
            return BadRequest("邮箱格式不正确");
        }

        var userName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        var subject = $"[ListenEase 反馈] {category}";
        var body = new StringBuilder();
        body.AppendLine("收到一条新的用户反馈：");
        body.AppendLine();
        body.AppendLine($"分类：{category}");
        if (!string.IsNullOrEmpty(name))
        {
            body.AppendLine($"称呼：{name}");
        }

        if (!string.IsNullOrEmpty(email))
        {
            body.AppendLine($"联系邮箱：{email}");
        }

        if (!string.IsNullOrEmpty(userName))
        {
            body.AppendLine($"登录用户：{userName}");
        }

        if (!string.IsNullOrEmpty(userId))
        {
            body.AppendLine($"用户 ID：{userId}");
        }

        body.AppendLine();
        body.AppendLine("反馈内容：");
        body.AppendLine(message);

        var recipient = feedbackSettings.RecipientEmail;
        var subjectCopy = subject;
        var bodyCopy = body.ToString();

        // 后台发信，避免 Outlook SMTP 慢导致前端 10s 超时
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                await emailSender.SendEmailAsync(recipient, subjectCopy, bodyCopy);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Background feedback email failed");
            }
        });

        return Ok(new { code = 200, message = "反馈已提交，感谢你的建议！" });
    }
}

public class SubmitFeedbackRequest
{
    public string? Category { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string Message { get; set; } = string.Empty;
}
