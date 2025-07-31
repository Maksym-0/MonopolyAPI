namespace Monopoly.Models.Request
{
    public class CreateRoomRequest
    {
        public int MaxNumberOfPlayers { get; set; }
        public string? Password { get; set; }
    }
}
