namespace GameClientStore.Services
{
    public interface IUserGamesService
    {
        Task<bool> AddOrUpdateUserGamesAsync(string username, List<string> gameNames);
        Task<bool> ClearUserGamesAsync(string username);
        Task<List<string>?> GetAllGamesAsync();
        Task<List<string>?> GetAllUsersAsync();
        Task<List<string>?> GetUserGamesByNameAsync(string username);
    }
}