using System;

namespace ChessBackend.Models
{
    public class GameRoom
    {
        public string Id { get; set; }
        public ChessGame Game { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public GameRoom(string id)
        {
            Id = id;
            Game = new ChessGame();
        }
    }
}
