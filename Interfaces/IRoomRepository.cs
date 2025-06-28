using Monopoly.Models;

namespace Monopoly.Abstractions
{
    public interface IRoomRepository
    {
        Task InsertRoomAsync(Room room);
        Task<List<Room>> ReadRoomListAsync();
        Task<Room> ReadRoomAsync(string roomId);
        Task UpdateRoomAsync(Room room);
        Task DeleteRoomAsync(string roomId);
    }
}
