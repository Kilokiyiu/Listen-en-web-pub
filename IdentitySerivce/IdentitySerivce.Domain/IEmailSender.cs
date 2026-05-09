namespace IdentitySerivce.Domain;

public interface IEmailSender
{
    public Task SendEmailAsync(string toEmail, string subject, string message);   
}