namespace Monopoly.Models.RoomModels
{
    public class Room
    {
        public string RoomId { get; set; }
        public int MaxNumberOfPlayers { get; set; }
        public int CountOfPlayers { get; set; }
        public string? Password { get; set; }
        public bool InGame { get; set; }
        public Room(string roomId, int maxNumberOfPlayers, int countOfPlayers)
        {
            RoomId = roomId;
            MaxNumberOfPlayers = maxNumberOfPlayers;
            CountOfPlayers = countOfPlayers;
            Password = null;
            InGame = false;
        }
        public Room(string roomId, int maxNumberOfPlayers, int countOfPlayers, string password)
        {
            RoomId = roomId;
            MaxNumberOfPlayers = maxNumberOfPlayers;
            CountOfPlayers = countOfPlayers;
            Password = password;
            InGame = false;
        }
        public Room() { }
    }
}
