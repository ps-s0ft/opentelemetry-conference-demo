namespace GameClientPublisher.Models.ViewModel
{
    public class UserGameDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
    }
}
