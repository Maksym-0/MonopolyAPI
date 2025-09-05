using Monopoly.Models.ApiResponse;

namespace Monopoly.Interfaces.IServices
{
    public interface IRoomService
    {
        Task<List<RoomDto>> GetAllRoomsAsync();
        Task<RoomDto> CreateRoomAsync(int maxNumberOfPlayers, string? password, string accountId, string accountName);
        Task<RoomDto> JoinRoomAsync(string roomId, string? password, string accountId, string accountName);
        Task<QuitRoomDto> QuitRoomAsync(string accountId);
    }
}
