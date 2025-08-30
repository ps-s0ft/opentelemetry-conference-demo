using GameClientPublisher.Infrastructure.DataBaseContext;
using GameClientPublisher.Models.DB;
using GameClientPublisher.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace GameClientPublisher.Services
{
    public class ManagementService : IManagementService
    {
        private readonly GameDbContext _context;
        private readonly ILogger<ManagementService> _logger;

        public ManagementService(GameDbContext context, ILogger<ManagementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Game>> GetAllGamesAsync()
        {
            _logger.LogInformation("Fetching all games");
            return await _context.Games.ToListAsync();
        }

        public async Task<Game> AddGameAsync(string name)
        {
            var game = new Game { Name = name };
            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Added new game {@Game}", game);
            return game;
        }

        public async Task<bool> DeleteGameAsync(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                _logger.LogWarning("Game with id={Id} not found for deletion", id);
                return false;
            }

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted game {@Game}", game);
            return true;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            _logger.LogInformation("Fetching all users");
            return await _context.Users.ToListAsync();
        }

        public async Task<User> AddUserAsync(string name)
        {
            var user = new User { Username = name };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Added new user {@User}", user);
            return user;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with id={Id} not found for deletion", id);
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted user {@User}", user);
            return true;
        }

        public async Task<IEnumerable<object>?> GetUserGamesAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserGames)
                .ThenInclude(ug => ug.Game)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning("User with id={Id} not found when fetching games", userId);
                return null;
            }

            return user.UserGames
                .Select(ug => new { ug.Game.Id, ug.Game.Name })
                .ToList();
        }

        public async Task<UserGameDto?> AddGameToUserAsync(int userId, int gameId)
        {
            var user = await _context.Users.FindAsync(userId);
            var game = await _context.Games.FindAsync(gameId);

            if (user == null || game == null)
            {
                _logger.LogWarning("Cannot add gameId={GameId} to userId={UserId}: user or game not found", gameId, userId);
                return null;
            }

            var userGame = new UserGame { UserId = userId, GameId = gameId };
            _context.UserGames.Add(userGame);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Added gameId={GameId} to userId={UserId}", gameId, userId);

            return new UserGameDto
            {
                UserId = user.Id,
                Username = user.Username,
                GameId = game.Id,
                GameName = game.Name
            };
        }

        public async Task<bool> RemoveGameFromUserAsync(int userId, int gameId)
        {
            var userGame = await _context.UserGames
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == gameId);

            if (userGame == null)
            {
                _logger.LogWarning("UserGame not found for userId={UserId}, gameId={GameId}", userId, gameId);
                return false;
            }

            _context.UserGames.Remove(userGame);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Removed gameId={GameId} from userId={UserId}", gameId, userId);
            return true;
        }
    }
}
