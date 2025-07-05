using Npgsql;
using Monopoly.Interfaces.IDatabases;
using Monopoly.Models.RoomModels;

namespace Monopoly.Database
{
    public class DBRoom : IRoomRepository
    {
        private readonly NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
        public async Task InsertRoomAsync(Room room)
        {
            var sql = $"INSERT INTO PUBLIC.\"{Constants.DBroomName}\" (\"RoomId\", \"MaxNumberOfPlayers\", \"CountOfPlayers\", \"Password\", \"InGame\")" +
                "VALUES (@roomId, @maxNumberOfPlayers, @countOfPlayers, @password, @inGame)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            AddWithValue(cmd, room);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task<List<Room>> ReadRoomListAsync()
        {
            var sql = $"SELECT \"RoomId\", \"MaxNumberOfPlayers\", \"CountOfPlayers\", \"Password\", \"InGame\" " +
                $"FROM public.\"{Constants.DBroomName}\"";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            List<Room> rooms = new List<Room>();
            await _connection.OpenAsync();
            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();
            
            while (await npgsqlData.ReadAsync())
            {
                Room room = ConstructRoom(npgsqlData);
                rooms.Add(room);
            }
            await _connection.CloseAsync();
            return rooms;
        }
        public async Task<Room> ReadRoomAsync(string roomId)
        {
            var sql = $"SELECT \"RoomId\", \"MaxNumberOfPlayers\", \"CountOfPlayers\", \"Password\", \"InGame\" " +
                $"FROM public.\"{Constants.DBroomName}\" " +
                $"WHERE \"RoomId\" = @roomId";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("roomId", roomId);
            
            await _connection.OpenAsync();
            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();
            
            if (!await npgsqlData.ReadAsync())
            {
                await _connection.CloseAsync();
                throw new Exception("Кімнату не знайдено");
            }
            Room room = ConstructRoom(npgsqlData);
            await _connection.CloseAsync();
            
            return room;
        }
        public async Task UpdateRoomAsync(Room room)
        {
            var sql = $"UPDATE public.\"{Constants.DBroomName}\" " +
                "SET \"CountOfPlayers\" = @countOfPlayers, \"MaxNumberOfPlayers\" = @maxNumberOfPlayers, \"InGame\" = @inGame, \"Password\" = @password " +
                $"WHERE \"RoomId\" = @roomId";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            AddWithValue(cmd, room);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task DeleteRoomAsync(string roomId)
        {
            var sql = $"DELETE FROM public.\"{Constants.DBroomName}\" " +
                "WHERE \"RoomId\" = @roomId";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("roomId", roomId);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }

        public async Task<bool> SearchRoomWithIdAsync(string roomId)
        {
            var sql = $"SELECT * FROM public.\"{Constants.DBroomName}\" where \"RoomId\" = @roomId";

            using NpgsqlCommand cmd = new(sql, _connection);
            cmd.Parameters.AddWithValue("roomId", roomId);

            await _connection.OpenAsync();
            using NpgsqlDataReader sqlData = await cmd.ExecuteReaderAsync();
            bool data = await sqlData.ReadAsync();
            await _connection.CloseAsync();

            return data;
        }

        private Room ConstructRoom(NpgsqlDataReader npgsqlData)
        {
            Room room;
            if (npgsqlData.IsDBNull(3))
            {
                room = new Room
                {
                    RoomId = npgsqlData.GetString(0),
                    MaxNumberOfPlayers = npgsqlData.GetInt32(1),
                    CountOfPlayers = npgsqlData.GetInt32(2),
                    Password = null,
                    InGame = npgsqlData.GetBoolean(4)
                };
            }
            else
            {
                room = new Room
                {
                    RoomId = npgsqlData.GetString(0),
                    MaxNumberOfPlayers = npgsqlData.GetInt32(1),
                    CountOfPlayers = npgsqlData.GetInt32(2),
                    Password = npgsqlData.GetString(3),
                    InGame = npgsqlData.GetBoolean(4)
                };
            }
            return room;
        }
        private void AddWithValue(NpgsqlCommand cmd, Room room)
        {
            cmd.Parameters.AddWithValue("roomId", room.RoomId);
            if (room.Password == null)
                cmd.Parameters.AddWithValue("password", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("password", room.Password);
            cmd.Parameters.AddWithValue("countOfPlayers", room.CountOfPlayers);
            cmd.Parameters.AddWithValue("maxNumberOfPlayers", room.MaxNumberOfPlayers);
            cmd.Parameters.AddWithValue("inGame", room.InGame);
        }
    }
}
