using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using KibanaEntegrationBackendTest.Models.ViewModels;
using System.Text;
using KibanaEntegrationBackendTest.Services.Interfaces;

namespace KibanaEntegrationBackendTest.Services;

public class DashboardService : IDashboardService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DashboardService> _logger;
    private readonly string _baseUrl;
    private readonly string _authHeader;

    public DashboardService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<DashboardService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _baseUrl = _configuration.GetValue<string>("Kibana:BaseUrl")!;
        var username = _configuration.GetValue<string>("Kibana:Username");
        var password = _configuration.GetValue<string>("Kibana:Password");
        _authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
    }

    public async Task<List<DashboardViewModel>> GetAllDashboardsAsync()
    {
        var allDashboards = new List<DashboardViewModel>();
        int page = 1;
        int perPage = 20;
        int total = 0;

        while (true)
        {
            var dashboardsJson = await GetDashboardsJsonAsync(page, perPage);

            if (page == 1)
            {
                total = dashboardsJson.GetProperty("total").GetInt32();
            }

            var currentDashboards = dashboardsJson.GetProperty("saved_objects")
                .EnumerateArray()
                .Select(d => new DashboardViewModel
                {
                    Id = d.GetProperty("id").GetString()!,
                    Title = d.GetProperty("attributes").GetProperty("title").GetString()!,
                    Description = d.GetProperty("attributes").TryGetProperty("description", out var desc) ? desc.GetString() : null,
                    LastUpdated = d.TryGetProperty("updated_at", out var updatedAt) ? updatedAt.GetString() : null
                });

            allDashboards.AddRange(currentDashboards);

            if (page * perPage >= total)
            {
                break;
            }

            page++;
        }

        return allDashboards;
    }

    public async Task<List<DashboardViewModel>> GetDashboardsByWidgetIdAsync(string widgetId)
    {
        var dashboards = new List<DashboardViewModel>();
        int page = 1;
        int perPage = 20;
        int total = 0;

        while (true)
        {
            var dashboardsJson = await GetDashboardsJsonAsync(page, perPage);

            if (page == 1)
            {
                total = dashboardsJson.GetProperty("total").GetInt32();
            }

            var currentDashboards = dashboardsJson
                .GetProperty("saved_objects")
                .EnumerateArray()
                .Where(dashboard =>
                    dashboard.GetProperty("references").EnumerateArray()
                             .Any(reference => reference.GetProperty("id").GetString() == widgetId))
                .Select(dashboard => new DashboardViewModel
                {
                    Id = dashboard.GetProperty("id").GetString()!,
                    Title = dashboard.GetProperty("attributes").GetProperty("title").GetString()!,
                    Description = dashboard.GetProperty("attributes").TryGetProperty("description", out var desc) ? desc.GetString() : null,
                    LastUpdated = dashboard.TryGetProperty("updated_at", out var updatedAt) ? updatedAt.GetString() : null
                })
                .ToList();

            dashboards.AddRange(currentDashboards);

            if (page * perPage >= total)
            {
                break;
            }

            page++;
        }

        return dashboards;
    }

    private async Task<JsonElement> GetDashboardsJsonAsync(int page, int perPage)
    {
        var requestUrl = $"{_baseUrl}/api/saved_objects/_find?type=dashboard&page={page}&per_page={perPage}";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _authHeader);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Dashboard retrieval failed with status: {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(content).RootElement;
    }
}
