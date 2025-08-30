using Microsoft.AspNetCore.Mvc;

namespace GameClientApi.Services
{
    public interface IUserGamesProxyService
    {
        Task<IActionResult> ProxyRequestAsync(HttpMethod method, string path, object? body, string? username, string activityName);
    }
}