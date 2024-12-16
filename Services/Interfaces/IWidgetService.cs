using KibanaEntegrationBackendTest.Models.ViewModels;
namespace KibanaEntegrationBackendTest.Services.Interfaces;

public interface IWidgetService
{
    Task<List<WidgetViewModel>> GetAllWidgetsAsync();
    Task<string?> GetWidgetTypeByIdAsync(string widgetId);
}