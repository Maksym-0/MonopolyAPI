using Monopoly.Models.RoomModels;

namespace Monopoly.Interfaces.IDatabases
{
    public interface IRoomRepository
    {
        Task InsertRoomAsync(Room room);
        Task<List<Room>> ReadRoomListAsync();
        Task<Room> ReadRoomAsync(string roomId);
        Task UpdateRoomAsync(Room room);
        Task DeleteRoomAsync(string roomId);

        Task<bool> SearchRoomWithIdAsync(string roomId);
    }
}
