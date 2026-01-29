namespace JdhPro.Web.Models;

public class Service
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool Featured { get; set; }
    public string ContentHtml { get; set; } = string.Empty;
    public List<string> Benefits { get; set; } = new();
    public List<string> UseCases { get; set; } = new();
}
