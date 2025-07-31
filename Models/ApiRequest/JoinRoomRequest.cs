namespace Monopoly.Models.Request
{
    public class JoinRoomRequest
    {
        public string RoomId { get; set; }
        public string? Password { get; set; }
    }
}
