namespace Monopoly.Models.RoomModels
{
    public class PlayerInRoom
    {
        public string RoomId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }

        public PlayerInRoom(string roomId, string id, string name)
        {
            RoomId = roomId;
            Id = id;
            Name = name;
        }
    }
}
