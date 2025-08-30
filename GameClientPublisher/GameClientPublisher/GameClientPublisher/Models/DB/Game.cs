namespace GameClientPublisher.Models.DB
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<UserGame> UserGames { get; set; } = new List<UserGame>();
    }
}
