using IdentitySerivce.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using MyCommonTools;

namespace IdentitySerivce.Infrastructure.Services;

public class CloudEmailSender : IEmailSender
{
    private const string SendCloudApiUrl = "https://api.sendcloud.net/apiv2/mail/send";

    private readonly ILogger<CloudEmailSender> logger;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IOptionsSnapshot<CloudEmailSettings> CloudEmailSettings;

    public CloudEmailSender(ILogger<CloudEmailSender> logger, IHttpClientFactory httpClientFactory,
        IOptionsSnapshot<CloudEmailSettings> CloadEmailSettings)
    {
        this.logger = logger;
        this.httpClientFactory = httpClientFactory;
        this.CloudEmailSettings = CloadEmailSettings;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        logger.LogInformation("SendCloud Email to {0}, title:{1}, message{2}", toEmail, subject, message);
        var postBody = new Dictionary<string, string>();
        postBody["apiUser"] = CloudEmailSettings.Value.ApiUser;
        postBody["apiKey"] = CloudEmailSettings.Value.ApiKey;
        postBody["from"] = CloudEmailSettings.Value.From;
        postBody["to"] = toEmail;
        postBody["subject"] = subject;
        postBody["plain"] = message;

        using (FormUrlEncodedContent httpContent = new FormUrlEncodedContent(postBody))
        {
            var httpClient = httpClientFactory.CreateClient();
            var responseMsg = await httpClient.PostAsync(SendCloudApiUrl, httpContent);
            if (!responseMsg.IsSuccessStatusCode)
            {
                throw new Exception($"发送邮件响应码错误: {responseMsg.StatusCode}");
            }
            var responseBody = await responseMsg.Content.ReadAsStringAsync();
            logger.LogInformation("SendCloud response: {ResponseBody}", responseBody);
            var responseModel = responseBody.ParseJson<SendCloudResponseModel>();
            if (responseModel == null)
            {
                throw new Exception($"发送邮件失败: 无法解析 SendCloud 响应");
            }
            if (!responseModel.Result)
            {
                throw new Exception($"发送邮件失败: {responseModel.Message ?? responseBody}");
            }
        }
    }
}