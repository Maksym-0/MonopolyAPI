using Monopoly.Models.RoomModels;
namespace Monopoly.Interfaces.IDatabases
{
    public interface IPlayerInRoomRepository
    {
        Task InsertPlayerInRoomAsync(PlayerInRoom playerInRoom);
        Task InsertPlayerInRoomListAsync(List<PlayerInRoom> playersInRoom);
        Task<List<PlayerInRoom>> ReadAllPlayerInRoomsListAsync();
        Task<List<PlayerInRoom>> ReadPlayerInRoomListAsync(string roomId);
        Task<PlayerInRoom> ReadPlayerInRoomAsync(string playerId);
        Task DeletePlayerInRoomAsync(string playerId);
        Task DeleteAllPlayersInRoomAsync(string playerId);

        Task<bool> SearchPlayerInRoomWithIdAsync(string playerId);
    }
}
