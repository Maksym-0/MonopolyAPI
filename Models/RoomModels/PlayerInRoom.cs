namespace Monopoly.Models.RoomModels
{
    public class PlayerInRoom
    {
        public string RoomId { get; }
        public string Id { get; }
        public string Name { get; }

        public PlayerInRoom(string roomId, string id, string name)
        {
            RoomId = roomId;
            Id = id;
            Name = name;
        }
    }
}
