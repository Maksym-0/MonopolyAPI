using Monopoly.Models.RoomModels;

namespace Monopoly.Models.ApiResponse
{
    public class RoomDto
    {
        public string RoomId { get; set; }
        public int MaxNumberOfPlayers { get; set; }
        public int NumberOfPlayers { get; set; }
        public List<PlayerInRoom> Players { get; set; }
        public bool HavePassword { get; set; }
        public bool InGame { get; set; }
        public RoomDto(Room room, List<PlayerInRoom> players)
        {
            RoomId = room.RoomId;
            MaxNumberOfPlayers = room.MaxNumberOfPlayers;
            NumberOfPlayers = room.CountOfPlayers;
            Players = players;
            if(room.Password != null)
                HavePassword = true;
            else HavePassword = false;
            InGame = room.InGame;
        }
    }
}
