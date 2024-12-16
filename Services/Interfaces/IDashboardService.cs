using KibanaEntegrationBackendTest.Models.ViewModels;

namespace KibanaEntegrationBackendTest.Services.Interfaces;
public interface IDashboardService
{
    Task<List<DashboardViewModel>> GetAllDashboardsAsync();
    Task<List<DashboardViewModel>> GetDashboardsByWidgetIdAsync(string widgetId);
}