namespace Aura.Dashboard.Services;

public interface IEmailSender
{
    Task SendAsync(string recipient, string subject, string body);
}
