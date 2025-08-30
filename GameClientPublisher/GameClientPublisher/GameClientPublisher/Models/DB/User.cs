namespace GameClientPublisher.Models.DB
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;

        public ICollection<UserGame> UserGames { get; set; } = new List<UserGame>();
    }
}
