namespace Monopoly.Models
{
    public class GameStatus
    {
        public string GameId {  get; set; }
        public List<Cell> Cells { get; set; }
        public List<Player> Players { get; set; }
        public GameStatus(string gameId, List<Cell> cells, List<Player> players) 
        { 
            GameId = gameId;
            Cells = cells;
            Players = players;
        }
    }
}
