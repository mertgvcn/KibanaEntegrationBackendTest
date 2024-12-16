namespace KibanaEntegrationBackendTest.Models.ViewModels;

public class WidgetViewModel
{
    public string Id { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public string? LastUpdated { get; set; }
}
