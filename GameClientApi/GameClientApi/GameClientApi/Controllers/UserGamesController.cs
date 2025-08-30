using GameClientApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameClientApi.Controllers
{
    [ApiController]
    [Route("user-games")]
    public class UserGamesController : ControllerBase
    {
        private readonly IUserGamesProxyService _proxyService;
        private readonly ILogger<UserGamesController> _logger;

        public UserGamesController(IUserGamesProxyService proxyService, ILogger<UserGamesController> logger)
        {
            _proxyService = proxyService;
            _logger = logger;
        }

        [HttpPost("{username}")]
        public Task<IActionResult> AddOrUpdateUserGames(string username, [FromBody] List<string> gameNames)
        {
            _logger.LogInformation("AddOrUpdateUserGames requested for user={User}", username);
            return _proxyService.ProxyRequestAsync(HttpMethod.Post, $"/user-games/{username}", gameNames, username, "AddOrUpdateUserGames");
        }

        [HttpDelete("{username}")]
        public Task<IActionResult> ClearUserGames(string username)
        {
            _logger.LogInformation("ClearUserGames requested for user={User}", username);
            return _proxyService.ProxyRequestAsync(HttpMethod.Delete, $"/user-games/{username}", null, username, "ClearUserGames");
        }

        [HttpGet("{username}")]
        public Task<IActionResult> GetUserGames(string username)
        {
            _logger.LogInformation("GetUserGames requested for user={User}", username);
            return _proxyService.ProxyRequestAsync(HttpMethod.Get, $"/user-games/{username}", null, username, "GetUserGames");
        }

        [HttpGet("games")]
        public Task<IActionResult> GetAllGames()
        {
            _logger.LogInformation("GetAllGames requested");
            return _proxyService.ProxyRequestAsync(HttpMethod.Get, "/user-games/games", null, null, "GetAllGames");
        }

        [HttpGet("users")]
        public Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation("GetAllUsers requested");
            return _proxyService.ProxyRequestAsync(HttpMethod.Get, "/user-games/users", null, null, "GetAllUsers");
        }
    }
}
