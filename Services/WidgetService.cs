using KibanaEntegrationBackendTest.Models.ViewModels;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using KibanaEntegrationBackendTest.Services.Interfaces;

namespace KibanaEntegrationBackendTest.Services;

public class WidgetService : IWidgetService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WidgetService> _logger;
    private readonly string _baseUrl;
    private readonly string _authHeader;

    public WidgetService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<WidgetService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _baseUrl = _configuration.GetValue<string>("Kibana:BaseUrl")!;
        var username = _configuration.GetValue<string>("Kibana:Username");
        var password = _configuration.GetValue<string>("Kibana:Password");
        _authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
    }

    public async Task<List<WidgetViewModel>> GetAllWidgetsAsync()
    {
        var allWidgets = new List<WidgetViewModel>();
        int page = 1;
        int perPage = 20;
        int total = 0;

        while (true)
        {
            var widgetsJson = await GetWidgetsJsonAsync(page, perPage);

            if (page == 1)
            {
                total = widgetsJson.GetProperty("total").GetInt32();
            }

            var currentWidget = widgetsJson.GetProperty("saved_objects")
                .EnumerateArray()
                .Select(v => new WidgetViewModel
                {
                    Id = v.GetProperty("id").GetString()!,
                    Type = v.GetProperty("type").GetString()!,
                    Title = v.GetProperty("attributes").GetProperty("title").GetString()!,
                    Description = v.GetProperty("attributes").TryGetProperty("description", out var desc) ? desc.GetString() : null,
                    LastUpdated = v.TryGetProperty("updated_at", out var updatedAt) ? updatedAt.GetString() : null
                });

            allWidgets.AddRange(currentWidget);

            if (page * perPage >= total)
            {
                break;
            }

            page++;
        }

        return allWidgets;
    }

    public async Task<string?> GetWidgetTypeByIdAsync(string widgetId)
    {
        string[] types = { "visualization", "lens" };
        foreach (var type in types)
        {
            var requestUrl = $"{_baseUrl}/api/saved_objects/{type}/{widgetId}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _authHeader);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var widgetJson = JsonDocument.Parse(content).RootElement;

                return widgetJson.GetProperty("type").GetString();
            }
        }

        _logger.LogWarning($"Widget with ID {widgetId} not found in supported types.");
        return null;
    }

    private async Task<JsonElement> GetWidgetsJsonAsync(int page, int perPage)
    {
        var requestUrl = $"{_baseUrl}/api/saved_objects/_find?type=lens&type=visualization&page={page}&per_page={perPage}";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _authHeader);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to retrieve widgets.");
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(content).RootElement;
    }

}
