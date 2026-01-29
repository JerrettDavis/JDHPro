using System.Text.Json;

namespace JdhPro.Web.Services;

public class ContactProviderFactory
{
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private string? _providerType;

    public ContactProviderFactory(HttpClient httpClient, IServiceProvider serviceProvider)
    {
        _httpClient = httpClient;
        _serviceProvider = serviceProvider;
    }

    public async Task<IContactProvider> GetProviderAsync()
    {
        await EnsureProviderTypeLoadedAsync();

        return _providerType?.ToLowerInvariant() switch
        {
            "mxroute" => _serviceProvider.GetRequiredService<MxRouteContactProvider>(),
            "mailto" => _serviceProvider.GetRequiredService<MailtoContactProvider>(),
            _ => _serviceProvider.GetRequiredService<MailtoContactProvider>() // Default to mailto
        };
    }

    private async Task EnsureProviderTypeLoadedAsync()
    {
        if (_providerType != null) return;

        try
        {
            var json = await _httpClient.GetStringAsync("content/config/contact.json");
            var config = JsonSerializer.Deserialize<ContactConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _providerType = config?.Provider ?? "mailto";
        }
        catch
        {
            _providerType = "mailto"; // Default to mailto on any error
        }
    }

    private class ContactConfig
    {
        public string Provider { get; set; } = string.Empty;
    }
}
