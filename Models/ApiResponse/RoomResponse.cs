using Monopoly.Models.RoomModels;

namespace Monopoly.Models.APIResponse
{
    public class RoomResponse
    {
        public string RoomId { get; set; }
        public int MaxNumberOfPlayers { get; set; }
        public int NumberOfPlayers { get; set; }
        public List<PlayerInRoom> PlayerNames { get; set; }
        public bool HavePassword { get; set; }
        public bool InGame { get; set; }
        public RoomResponse(Room room, List<PlayerInRoom> players)
        {
            RoomId = room.RoomId;
            MaxNumberOfPlayers = room.MaxNumberOfPlayers;
            NumberOfPlayers = room.CountOfPlayers;
            PlayerNames = players;
            if(room.Password != null)
                HavePassword = true;
            else HavePassword = false;
            InGame = room.InGame;
        }
    }
}
