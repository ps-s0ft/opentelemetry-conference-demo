using GameClientPublisher.Models.DB;
using GameClientPublisher.Models.ViewModel;

namespace GameClientPublisher.Services
{
    public interface IManagementService
    {
        Task<IEnumerable<Game>> GetAllGamesAsync();
        Task<Game> AddGameAsync(string name);
        Task<bool> DeleteGameAsync(int id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> AddUserAsync(string name);
        Task<bool> DeleteUserAsync(int id);
        Task<IEnumerable<object>?> GetUserGamesAsync(int userId);
        Task<UserGameDto?> AddGameToUserAsync(int userId, int gameId);
        Task<bool> RemoveGameFromUserAsync(int userId, int gameId);
    }
}
