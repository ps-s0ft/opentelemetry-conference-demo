using GameClientPublisher.Infrastructure.DataBaseContext;
using GameClientPublisher.Models.DB;

namespace GameClientPublisher.Infrastructure.Extensions
{
    public static class SetDBDataExtension
    {
        public static void SetDbDefaultData(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
            db.Database.EnsureCreated();

            if (!db.Users.Any())
            {
                var users = new List<User>
                {
                    new User { Username = "alice" },
                    new User { Username = "bob" },
                    new User { Username = "charlie" },
                    new User { Username = "dave" },
                    new User { Username = "eve" },
                    new User { Username = "frank" },
                    new User { Username = "grace" },
                    new User { Username = "heidi" },
                    new User { Username = "alex" },
                    new User { Username = "judy" }
                };

                var games = new List<Game>
                {
                    new Game { Name = "The Witcher 3" },
                    new Game { Name = "Cyberpunk 2077" },
                    new Game { Name = "Minecraft" },
                    new Game { Name = "The Last of Us Part I" },
                    new Game { Name = "Hollow Knight" },
                    new Game { Name = "Celeste" },
                    new Game { Name = "Among Us" },
                    new Game { Name = "Factorio" },
                    new Game { Name = "Terraria" },
                    new Game { Name = "Hades" },
                    new Game { Name = "CS2" }
                };

                db.Users.AddRange(users);
                db.Games.AddRange(games);
                db.SaveChanges();

                var random = new Random();
                foreach (var user in users)
                {
                    var assignedGames = games.OrderBy(x => random.Next()).Take(3).ToList();
                    foreach (var game in assignedGames)
                    {
                        db.UserGames.Add(new UserGame
                        {
                            UserId = user.Id,
                            GameId = game.Id
                        });
                    }
                }

                db.SaveChanges();
            }
        }
    }
}
