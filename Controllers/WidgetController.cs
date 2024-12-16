using KibanaEntegrationBackendTest.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KibanaEntegrationBackendTest.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class WidgetController : ControllerBase
{
    private readonly IWidgetService _widgetService;

    public WidgetController(IWidgetService widgetService)
    {
        _widgetService = widgetService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllWidgets()
    {
        try
        {
            var result = await _widgetService.GetAllWidgetsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetWidgetTypeById([FromQuery] string widgetId)
    {
        try
        {
            var widgetType = await _widgetService.GetWidgetTypeByIdAsync(widgetId);
            if (widgetType == null)
            {
                return NotFound($"Widget with ID {widgetId} not found in supported types.");
            }
            return Ok(widgetType);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
