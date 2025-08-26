using Monopoly.Models.GameModels;

namespace Monopoly.Models.ApiResponse
{
    public class LeaveGameDto
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        
        public int RemainingPlayers { get; set; }
        public Player? Winner { get; set; }

        public bool IsGameOver { get; set; }
    }
}
