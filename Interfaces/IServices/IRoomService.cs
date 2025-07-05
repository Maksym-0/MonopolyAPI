using Monopoly.Database;
using Monopoly.Models.APIResponse;
using Monopoly.Models.RoomModels;

namespace Monopoly.Interfaces.IServices
{
    public interface IRoomService
    {
        Task<List<RoomResponse>> GetAllRoomsAsync();
        Task<RoomResponse> CreateRoomAsync(int maxNumberOfPlayers, string? password, string accountId, string accountName);
        Task<RoomResponse> JoinRoomAsync(string roomId, string? password, string accountId, string accountName);
        Task<string> QuitRoomAsync(string accountId);
    }
}
