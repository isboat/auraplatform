using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Aura.Dashboard.Services;

public sealed class YahooSmtpEmailSender(IOptions<YahooMailOptions> options) : IEmailSender
{
    public async Task SendAsync(string recipient, string subject, string body)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.EmailAddress) || string.IsNullOrWhiteSpace(settings.Passkey))
            throw new InvalidOperationException("YahooMail:EmailAddress and YahooMail:Passkey must be configured.");

        using var message = new MailMessage(settings.EmailAddress, recipient, subject, body);
        using var smtpClient = new SmtpClient("smtp.mail.yahoo.com", 587)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(settings.EmailAddress, settings.Passkey)
        };

        await smtpClient.SendMailAsync(message);
    }
}
