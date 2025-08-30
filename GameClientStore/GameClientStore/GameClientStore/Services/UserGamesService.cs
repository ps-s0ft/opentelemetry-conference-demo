using GameClientStore.Models;

namespace GameClientStore.Services
{
    public class UserGamesService : IUserGamesService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UserGamesService> _logger;

        public UserGamesService(IHttpClientFactory httpClientFactory, ILogger<UserGamesService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        private HttpClient CreateClient()
        {
            return _httpClientFactory.CreateClient("GameClientPublisher");
        }

        private async Task<UserDto?> FetchUserAsync(string username)
        {
            _logger.LogInformation("Fetching user by name: {Username}", username);

            try
            {
                var client = CreateClient();
                var users = await client.GetFromJsonAsync<List<UserDto>>("/management/users");
                var user = users?.FirstOrDefault(u => u.Username == username);

                if (user != null)
                {
                    _logger.LogDebug("Found user {Username} with id {UserId}", username, user.Id);
                }
                else
                {
                    _logger.LogDebug("User {Username} not found", username);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch users when looking for {Username}", username);
                throw;
            }
        }

        private async Task<UserDto?> CreateUserAsync(string username)
        {
            _logger.LogInformation("Creating user: {Username}", username);

            try
            {
                var client = CreateClient();
                var response = await client.PostAsync($"/management/users?name={Uri.EscapeDataString(username)}", null);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Create user {Username} failed with status {StatusCode}: {Content}", username, (int)response.StatusCode, content);
                    throw new InvalidOperationException($"Failed to create user {username}");
                }

                var created = await response.Content.ReadFromJsonAsync<UserDto>();
                _logger.LogInformation("User {Username} created with id {UserId}", username, created?.Id);
                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when creating user {Username}", username);
                throw;
            }
        }

        private async Task<GameDto?> FetchGameAsync(string gameName)
        {
            _logger.LogDebug("Looking up game: {GameName}", gameName);

            try
            {
                var client = CreateClient();
                var games = await client.GetFromJsonAsync<List<GameDto>>("/management/games");
                var game = games?.FirstOrDefault(g => g.Name.Equals(gameName, StringComparison.OrdinalIgnoreCase));

                if (game != null)
                {
                    _logger.LogDebug("Found game {GameName} with id {GameId}", gameName, game.Id);
                }
                else
                {
                    _logger.LogDebug("Game {GameName} not found", gameName);
                }

                return game;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch games when looking for {GameName}", gameName);
                throw;
            }
        }

        private async Task<GameDto?> CreateGameAsync(string gameName)
        {
            _logger.LogInformation("Creating game: {GameName}", gameName);

            try
            {
                var client = CreateClient();
                var response = await client.PostAsync($"/management/games?name={Uri.EscapeDataString(gameName)}", null);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Create game {GameName} failed with status {StatusCode}: {Content}", gameName, (int)response.StatusCode, content);
                    throw new InvalidOperationException($"Failed to create game {gameName}");
                }

                var created = await response.Content.ReadFromJsonAsync<GameDto>();
                _logger.LogInformation("Game {GameName} created with id {GameId}", gameName, created?.Id);
                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when creating game {GameName}", gameName);
                throw;
            }
        }

        private async Task<bool> LinkUserGameAsync(int userId, int gameId)
        {
            _logger.LogDebug("Linking game {GameId} to user {UserId}", gameId, userId);

            try
            {
                var client = CreateClient();
                var response = await client.PostAsync($"/management/users/{userId}/games/{gameId}", null);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Link user {UserId} with game {GameId} failed: {StatusCode} {Content}", userId, gameId, (int)response.StatusCode, content);
                }
                else
                {
                    _logger.LogInformation("Linked game {GameId} to user {UserId}", gameId, userId);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when linking game {GameId} to user {UserId}", gameId, userId);
                throw;
            }
        }

        private async Task<List<GameDto>?> FetchUserGamesAsync(int userId)
        {
            _logger.LogDebug("Fetching games for user id {UserId}", userId);

            try
            {
                var client = CreateClient();
                return await client.GetFromJsonAsync<List<GameDto>>($"/management/users/{userId}/games");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch games for user id {UserId}", userId);
                throw;
            }
        }

        private async Task UnlinkUserGameAsync(int userId, int gameId)
        {
            _logger.LogDebug("Unlinking game {GameId} from user {UserId}", gameId, userId);

            try
            {
                var client = CreateClient();
                await client.DeleteAsync($"/management/users/{userId}/games/{gameId}");
                _logger.LogInformation("Unlinked game {GameId} from user {UserId}", gameId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unlink game {GameId} from user {UserId}", gameId, userId);
                throw;
            }
        }

        public async Task<bool> AddOrUpdateUserGamesAsync(string username, List<string> gameNames)
        {
            _logger.LogInformation("AddOrUpdateUserGames called for user {Username} with {Count} games", username, gameNames?.Count ?? 0);

            var user = await FetchUserAsync(username);
            if (user == null)
            {
                user = await CreateUserAsync(username);
                if (user == null)
                {
                    _logger.LogError("Failed to create user {Username} after fetch returned null", username);
                    throw new InvalidOperationException($"Failed to create user {username}");
                }
            }

            foreach (var gameName in gameNames)
            {
                var game = await FetchGameAsync(gameName);
                if (game == null)
                {
                    game = await CreateGameAsync(gameName);
                    if (game == null)
                    {
                        _logger.LogError("Failed to create game {GameName} for user {Username}", gameName, username);
                        throw new InvalidOperationException($"Failed to create game {gameName}");
                    }
                }

                if (string.Equals(gameName, "fail", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError("Demo failure triggered for user {Username} when adding game {GameName}", username, gameName);
                    throw new InvalidOperationException("Looks like you have exception during demo. Try to find me!");
                }

                var success = await LinkUserGameAsync(user.Id, game.Id);
                if (!success)
                {
                    _logger.LogError("Failed to link game {GameId} to user {UserId}", game.Id, user.Id);
                    throw new InvalidOperationException($"Failed to add game {gameName} for user {username}");
                }
            }

            _logger.LogInformation("Successfully processed {Count} games for user {Username}", gameNames.Count, username);
            return true;
        }

        public async Task<bool> ClearUserGamesAsync(string username)
        {
            _logger.LogInformation("ClearUserGames called for user {Username}", username);

            var user = await FetchUserAsync(username);
            if (user == null)
            {
                _logger.LogDebug("User {Username} not found when attempting to clear games", username);
                return false;
            }

            var userGames = await FetchUserGamesAsync(user.Id);
            if (userGames == null || !userGames.Any())
            {
                _logger.LogDebug("No games found for user {Username} when attempting to clear", username);
                return false;
            }

            foreach (var game in userGames)
            {
                await UnlinkUserGameAsync(user.Id, game.Id);
            }

            _logger.LogInformation("Cleared {Count} games for user {Username}", userGames.Count, username);
            return true;
        }

        public async Task<List<string>?> GetUserGamesByNameAsync(string username)
        {
            _logger.LogDebug("GetUserGamesByNameAsync called for user {Username}", username);

            var user = await FetchUserAsync(username);
            if (user == null)
            {
                _logger.LogDebug("User {Username} not found", username);
                return null;
            }

            var games = await FetchUserGamesAsync(user.Id);
            var names = games?.Select(g => g.Name).ToList();
            _logger.LogDebug("Returning {Count} games for user {Username}", names?.Count ?? 0, username);
            return names;
        }

        public async Task<List<string>?> GetAllGamesAsync()
        {
            _logger.LogDebug("GetAllGamesAsync called");
            var client = CreateClient();

            try
            {
                var games = await client.GetFromJsonAsync<List<GameDto>>("/management/games");
                var names = games?.Select(g => g.Name).ToList();
                _logger.LogDebug("Found {Count} total games", names?.Count ?? 0);
                return names;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all games");
                throw;
            }
        }

        public async Task<List<string>?> GetAllUsersAsync()
        {
            _logger.LogDebug("GetAllUsersAsync called");
            var client = CreateClient();

            try
            {
                var users = await client.GetFromJsonAsync<List<UserDto>>("/management/users");
                var names = users?.Select(u => u.Username).ToList();
                _logger.LogDebug("Found {Count} total users", names?.Count ?? 0);
                return names;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all users");
                throw;
            }
        }
    }
}
