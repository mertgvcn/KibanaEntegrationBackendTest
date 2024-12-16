using KibanaEntegrationBackendTest.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace KibanaEntegrationBackendTest.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDashboards()
    {
        try
        {
            var result = await _dashboardService.GetAllDashboardsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardsUsingWidgetId([FromQuery] string widgetId)
    {
        try
        {
            var dashboards = await _dashboardService.GetDashboardsByWidgetIdAsync(widgetId);
            return Ok(dashboards);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
