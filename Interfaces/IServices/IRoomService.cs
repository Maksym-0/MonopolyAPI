using Monopoly.Database;
using Monopoly.Models.APIResponse;
using Monopoly.Models.RoomModels;

namespace Monopoly.Interfaces.IServices
{
    public interface IRoomService
    {
        Task<List<RoomDto>> GetAllRoomsAsync();
        Task<RoomDto> CreateRoomAsync(int maxNumberOfPlayers, string? password, string accountId, string accountName);
        Task<RoomDto> JoinRoomAsync(string roomId, string? password, string accountId, string accountName);
        Task<string> QuitRoomAsync(string accountId);
    }
}
