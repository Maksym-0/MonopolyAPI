namespace Monopoly.Models
{
    public class Room
    {
        public string RoomId { get; set; }
        public int MaxNumberOfPlayers { get; set; }
        public int NumberOfPlayers { get; set; }
        public List<string>? PlayerNames { get; set; }
        public string? Password { get; set; }
        public bool InGame { get; set; }
        public Room(string roomId, int maxNumberOfPlayers, List<string> playerNames)
        {
            RoomId = roomId;
            MaxNumberOfPlayers = maxNumberOfPlayers;
            NumberOfPlayers = 0;
            PlayerNames = playerNames;
            Password = null;
            InGame = false;
        }
        public Room(string roomId, int maxNumberOfPlayers, List<string> playerNames, string password)
        {
            RoomId = roomId;
            MaxNumberOfPlayers = maxNumberOfPlayers;
            NumberOfPlayers = 0;
            PlayerNames = playerNames;
            Password = password;
            InGame = false;
        }
        public Room() { }
    }
}
