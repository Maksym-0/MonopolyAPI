namespace Monopoly.Models.ApiResponse
{
    public class QuitRoomDto
    {
        public bool IsRoomDeleted { get; set; }
       
        public string? PlayerName { get; set; }
        public string? Winner { get; set; }
        
        public int? RemainingPlayers { get; set; }

        public RoomDto? RoomDto { get; set; }
    }
}
