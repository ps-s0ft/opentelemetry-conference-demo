using GameClientPublisher.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameClientPublisher.Controllers
{
    [ApiController]
    [Route("management")]
    public class ManagementController : ControllerBase
    {
        private readonly IManagementService _service;
        private readonly ILogger<ManagementController> _logger;

        public ManagementController(IManagementService service, ILogger<ManagementController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("games")]
        public async Task<IActionResult> GetAllGames()
        {
            _logger.LogInformation("Fetching all games");
            var games = await _service.GetAllGamesAsync();
            return Ok(games);
        }

        [HttpPost("games")]
        public async Task<IActionResult> AddGame(string name)
        {
            var game = await _service.AddGameAsync(name);
            _logger.LogInformation("Game added: {GameName} (Id: {GameId})", game.Name, game.Id);
            return Ok(game);
        }

        [HttpDelete("games/{id}")]
        public async Task<IActionResult> DeleteGame(int id)
        {
            var deleted = await _service.DeleteGameAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Attempted to delete non-existing game with Id={GameId}", id);
                return NotFound();
            }

            _logger.LogInformation("Deleted game with Id={GameId}", id);
            return NoContent();
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation("Fetching all users");
            var users = await _service.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPost("users")]
        public async Task<IActionResult> AddUser(string name)
        {
            var user = await _service.AddUserAsync(name);
            _logger.LogInformation("User added: {Username} (Id: {UserId})", user.Username, user.Id);
            return Ok(user);
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _service.DeleteUserAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Attempted to delete non-existing user with Id={UserId}", id);
                return NotFound();
            }

            _logger.LogInformation("Deleted user with Id={UserId}", id);
            return NoContent();
        }

        [HttpGet("users/{userId}/games")]
        public async Task<IActionResult> GetUserGames(int userId)
        {
            var games = await _service.GetUserGamesAsync(userId);
            if (games == null)
            {
                _logger.LogWarning("User with Id={UserId} not found when fetching games", userId);
                return NotFound();
            }

            _logger.LogInformation("Fetched {Count} games for user Id={UserId}", games.Count(), userId);
            return Ok(games);
        }

        [HttpPost("users/{userId}/games/{gameId}")]
        public async Task<IActionResult> AddGameToUser(int userId, int gameId)
        {
            var dto = await _service.AddGameToUserAsync(userId, gameId);
            if (dto == null)
            {
                _logger.LogWarning("Failed to add gameId={GameId} to userId={UserId}", gameId, userId);
                return NotFound();
            }

            _logger.LogInformation("Added gameId={GameId} to userId={UserId}", gameId, userId);
            return Ok(dto);
        }

        [HttpDelete("users/{userId}/games/{gameId}")]
        public async Task<IActionResult> RemoveGameFromUser(int userId, int gameId)
        {
            var removed = await _service.RemoveGameFromUserAsync(userId, gameId);
            if (!removed)
            {
                _logger.LogWarning("Attempted to remove non-existing gameId={GameId} from userId={UserId}", gameId, userId);
                return NotFound();
            }

            _logger.LogInformation("Removed gameId={GameId} from userId={UserId}", gameId, userId);
            return NoContent();
        }
    }
}
