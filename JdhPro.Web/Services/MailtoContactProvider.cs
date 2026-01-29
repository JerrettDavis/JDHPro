using System.Text.Json;
using System.Web;

namespace JdhPro.Web.Services;

public class MailtoContactProvider : IContactProvider
{
    private readonly HttpClient _httpClient;
    private MailtoConfig? _config;

    public string ProviderName => "mailto";

    public MailtoContactProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ContactResult> SendContactAsync(ContactMessage message)
    {
        try
        {
            await EnsureConfigLoadedAsync();

            if (_config == null || string.IsNullOrEmpty(_config.To))
            {
                return new ContactResult
                {
                    Success = false,
                    Message = "Contact configuration not found",
                    ErrorDetails = "mailto configuration is missing or incomplete"
                };
            }

            var subject = _config.Subject
                .Replace("{name}", message.Name)
                .Replace("{email}", message.Email)
                .Replace("{company}", message.Company)
                .Replace("{service}", message.Service);

            var body = BuildEmailBody(message);
            var mailtoLink = $"mailto:{_config.To}?subject={HttpUtility.UrlEncode(subject)}&body={HttpUtility.UrlEncode(body)}";

            return new ContactResult
            {
                Success = true,
                Message = mailtoLink
            };
        }
        catch (Exception ex)
        {
            return new ContactResult
            {
                Success = false,
                Message = "Failed to generate contact link",
                ErrorDetails = ex.Message
            };
        }
    }

    private async Task EnsureConfigLoadedAsync()
    {
        if (_config != null) return;

        try
        {
            var json = await _httpClient.GetStringAsync("content/config/contact.json");
            var rootConfig = JsonSerializer.Deserialize<ContactConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _config = rootConfig?.Mailto ?? new MailtoConfig { To = "contact@jdhpro.com", Subject = "Website Contact" };
        }
        catch
        {
            _config = new MailtoConfig { To = "contact@jdhpro.com", Subject = "Website Contact" };
        }
    }

    private string BuildEmailBody(ContactMessage message)
    {
        var body = $"Name: {message.Name}\n";
        body += $"Email: {message.Email}\n";
        
        if (!string.IsNullOrEmpty(message.Company))
        {
            body += $"Company: {message.Company}\n";
        }
        
        if (!string.IsNullOrEmpty(message.Service))
        {
            body += $"Service Interest: {message.Service}\n";
        }
        
        body += $"\nMessage:\n{message.Message}";
        
        return body;
    }

    private class ContactConfig
    {
        public string Provider { get; set; } = string.Empty;
        public MailtoConfig? Mailto { get; set; }
    }

    private class MailtoConfig
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
    }
}
