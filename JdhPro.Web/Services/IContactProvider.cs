namespace JdhPro.Web.Services;

public interface IContactProvider
{
    Task<ContactResult> SendContactAsync(ContactMessage message);
    string ProviderName { get; }
}

public class ContactMessage
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
}

public class ContactResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorDetails { get; set; }
}
