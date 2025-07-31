using Monopoly.Models.GameModels;

namespace Monopoly.Models.APIResponse
{
    public class GameReponse
    {
        public string GameId {  get; set; }
        public List<Cell> Cells { get; set; }
        public List<Player> Players { get; set; }
        public GameReponse(string gameId, List<Cell> cells, List<Player> players) 
        { 
            GameId = gameId;
            Cells = cells;
            Players = players;
        }
    }
}
