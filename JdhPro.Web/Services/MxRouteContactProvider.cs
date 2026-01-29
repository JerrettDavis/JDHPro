namespace JdhPro.Web.Services;

public class MxRouteContactProvider : IContactProvider
{
    public string ProviderName => "mxroute";

    public Task<ContactResult> SendContactAsync(ContactMessage message)
    {
        // TODO: Implement MXRoute SMTP integration
        // TODO: Read SMTP configuration from contact.json (host, port, username, password, from address)
        // TODO: Use System.Net.Mail.SmtpClient or MailKit for sending
        // TODO: Handle SSL/TLS configuration
        // TODO: Add retry logic for transient failures
        // TODO: Implement proper error handling and logging

        throw new NotImplementedException(
            "MXRoute SMTP provider is not yet configured. " +
            "Please configure SMTP settings in contact.json or use the mailto provider.");
    }
}
