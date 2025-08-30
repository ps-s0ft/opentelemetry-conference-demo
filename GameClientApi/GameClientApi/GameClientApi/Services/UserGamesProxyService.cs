using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;

namespace GameClientApi.Services
{
    public class UserGamesProxyService : IUserGamesProxyService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ActivitySource _activitySource;
        private readonly Counter<int> _gamesRequestCounter;
        private readonly ILogger<UserGamesProxyService> _logger;

        public UserGamesProxyService(
            IHttpClientFactory httpClientFactory,
            ActivitySource activitySource,
            Meter meter,
            ILogger<UserGamesProxyService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _activitySource = activitySource;
            _gamesRequestCounter = meter.CreateCounter<int>(
                "games.request.count",
                description: "Number of requests to GameApi"
            );
            _logger = logger;
        }

        public async Task<IActionResult> ProxyRequestAsync(HttpMethod method, string path, object? body, string? username, string activityName)
        {
            using var activity = _activitySource.StartActivity(activityName);
            if (!string.IsNullOrEmpty(username))
            {
                activity?.SetTag("user.name", username);
            }

            _gamesRequestCounter.Add(1,
                new KeyValuePair<string, object?>("username", username ?? "all"),
                new KeyValuePair<string, object?>("request_path", path),
                new KeyValuePair<string, object?>("request_method", method.Method));

            try
            {
                var client = _httpClientFactory.CreateClient("GameClientStore");
                _logger.LogInformation("Sending {Method} request to {Path} for user={User}", method, path, username);

                HttpResponseMessage response = body != null
                    ? await client.PostAsJsonAsync(path, body)
                    : method == HttpMethod.Get
                        ? await client.GetAsync(path)
                        : await client.DeleteAsync(path);

                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("GameClientStore returned error {StatusCode} for {Path}: {Content}", response.StatusCode, path, content);

                    object? errorObj;
                    try
                    {
                        errorObj = JsonSerializer.Deserialize<JsonElement>(content);
                    }
                    catch
                    {
                        errorObj = content;
                    }

                    activity?.SetStatus(ActivityStatusCode.Error, "Downstream call failed");
                    activity?.SetTag("error.message", content);
                    return new ObjectResult(errorObj) { StatusCode = (int)response.StatusCode };
                }

                _logger.LogInformation("Request {Method} {Path} for user={User} succeeded", method, path, username);

                if (!string.IsNullOrWhiteSpace(content))
                {
                    try
                    {
                        var jsonResult = JsonSerializer.Deserialize<object>(content);
                        return new OkObjectResult(jsonResult);
                    }
                    catch
                    {
                        return new OkObjectResult(content);
                    }
                }

                return new OkResult();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling GameClientStore at {Path}", path);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                return new ObjectResult("Internal error when calling GameClientStore") { StatusCode = 500 };
            }
        }
    }
}
