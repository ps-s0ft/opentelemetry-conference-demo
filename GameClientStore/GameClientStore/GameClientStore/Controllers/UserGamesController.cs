using GameClientStore.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace GameClientStore.Controllers
{
    [ApiController]
    [Route("user-games")]
    public class UserGamesController : ControllerBase
    {
        private readonly IUserGamesService _userGamesService;

        public UserGamesController(IUserGamesService userGamesService)
        {
            _userGamesService = userGamesService;
        }

        [HttpPost("{username}")]
        public async Task<IActionResult> AddOrUpdateUserGames(string username, [FromBody] List<string> gameNames)
        {
            Log.Information("Received request to add/update games for user {Username}. Games: {@Games}", username, gameNames);

            try
            {
                var result = await _userGamesService.AddOrUpdateUserGamesAsync(username, gameNames);
                if (!result)
                {
                    Log.Warning("User {Username} not found while updating games", username);
                    return NotFound($"User {username} not found");
                }

                Log.Information("Successfully updated games for user {Username}", username);
                return Ok($"Games for {username} updated");
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Invalid argument when updating games for user {Username}", username);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex, "Operation failed while updating games for user {Username}", username);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> ClearUserGames(string username)
        {
            Log.Information("Received request to clear games for user {Username}", username);

            var result = await _userGamesService.ClearUserGamesAsync(username);
            if (!result)
            {
                Log.Warning("User {Username} not found or no games to clear", username);
                return NotFound($"User {username} not found or no games to clear");
            }

            Log.Information("Successfully cleared all games for user {Username}", username);
            return Ok($"All games for {username} removed");
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserGames(string username)
        {
            Log.Information("Received request to fetch games for user {Username}", username);

            var userGames = await _userGamesService.GetUserGamesByNameAsync(username);
            if (userGames == null)
            {
                Log.Warning("User {Username} not found when fetching games", username);
                return NotFound($"User {username} not found");
            }

            Log.Information("Successfully fetched {Count} games for user {Username}", userGames.Count, username);
            return Ok(userGames);
        }

        [HttpGet("games")]
        public async Task<IActionResult> GetAllGames()
        {
            Log.Information("Received request to fetch all games");

            var games = await _userGamesService.GetAllGamesAsync();
            Log.Information("Fetched {Count} total games", games?.Count ?? 0);

            return Ok(games);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            Log.Information("Received request to fetch all users");

            var users = await _userGamesService.GetAllUsersAsync();
            Log.Information("Fetched {Count} total users", users?.Count ?? 0);

            return Ok(users);
        }
    }
}
