using Monopoly.Interfaces.IDatabases;
using Monopoly.Models.RoomModels;
using Npgsql;

namespace Monopoly.Database
{
    public class DbPlayerInRoom : IPlayerInRoomRepository
    {
        public async Task InsertPlayerInRoomAsync(PlayerInRoom playerInRoom)
        {
            var sql = $"INSERT INTO PUBLIC.\"{Constants.DBplayerInRoomName}\" (\"RoomId\", \"Id\", \"Name\")" +
                "VALUES (@roomId, @id, @name)";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            AddWithValue(cmd, playerInRoom);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task InsertPlayerInRoomListAsync(List<PlayerInRoom> playersInRoom)
        {
            var sql = $"INSERT INTO PUBLIC.\"{Constants.DBplayerInRoomName}\" (\"RoomId\", \"Id\", \"Name\")" +
                "VALUES (@roomId, @id, @name)";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);
            await _connection.OpenAsync();
            foreach(PlayerInRoom playerInRoom in playersInRoom)
            {
                AddWithValue(cmd, playerInRoom);
                await cmd.ExecuteNonQueryAsync();
                cmd.Parameters.Clear();
            }

            await _connection.CloseAsync();
        }
        public async Task<List<PlayerInRoom>> ReadAllPlayerInRoomsListAsync()
        {
            var sql = $"SELECT \"RoomId\", \"Id\", \"Name\" " +
                $"FROM public.\"{Constants.DBplayerInRoomName}\"";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            List<PlayerInRoom> playersInRoom = new List<PlayerInRoom>();
            await _connection.OpenAsync();
            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();

            while (await npgsqlData.ReadAsync())
            {
                PlayerInRoom playerInRoom = ConstructPlayer(npgsqlData);
                playersInRoom.Add(playerInRoom);
            }
            await _connection.CloseAsync();
            return playersInRoom;
        }
        public async Task<List<PlayerInRoom>> ReadPlayerInRoomListAsync(string roomId)
        {
            var sql = $"SELECT \"RoomId\", \"Id\", \"Name\" " +
                $"FROM public.\"{Constants.DBplayerInRoomName}\" " +
                $"WHERE \"RoomId\" = @roomId";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            List<PlayerInRoom> playersInRoom = new List<PlayerInRoom>();
            cmd.Parameters.AddWithValue("roomId", roomId);
            await _connection.OpenAsync();
            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();

            while (await npgsqlData.ReadAsync())
            {
                PlayerInRoom playerInRoom = ConstructPlayer(npgsqlData);
                playersInRoom.Add(playerInRoom);
            }
            await _connection.CloseAsync();
            return playersInRoom;
        }
        public async Task<PlayerInRoom> ReadPlayerInRoomAsync(string playerId)
        {
            var sql = $"SELECT \"RoomId\", \"Id\", \"Name\" " +
                $"FROM public.\"{Constants.DBplayerInRoomName}\" " +
                $"WHERE \"Id\" = @id";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("id", playerId);

            await _connection.OpenAsync();
            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();

            if (!await npgsqlData.ReadAsync())
            {
                await _connection.CloseAsync();
                throw new Exception("Гравець у кімнаті не знайдений");
            }
            PlayerInRoom playerInRoom = ConstructPlayer(npgsqlData);
            await _connection.CloseAsync();

            return playerInRoom;
        }
        public async Task DeleteAllPlayersInRoomAsync(string roomId)
        {
            var sql = $"DELETE FROM public.\"{Constants.DBplayerInRoomName}\" " +
                "WHERE \"RoomId\" = @roomId";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("roomId", roomId);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task DeletePlayerInRoomAsync(string playerId)
        {
            var sql = $"DELETE FROM public.\"{Constants.DBplayerInRoomName}\" " +
                "WHERE \"Id\" = @id";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("id", playerId);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }

        public async Task<bool> SearchPlayerInRoomWithIdAsync(string playerId)
        {
            var sql = $"SELECT * FROM public.\"{Constants.DBplayerInRoomName}\" where \"Id\" = @id";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new(sql, _connection);
            cmd.Parameters.AddWithValue("id", playerId);

            await _connection.OpenAsync();
            NpgsqlDataReader sqlData = await cmd.ExecuteReaderAsync();
            bool data = await sqlData.ReadAsync();
            await _connection.CloseAsync();

            return data;
        }

        private PlayerInRoom ConstructPlayer(NpgsqlDataReader npgsqlData)
        {
            PlayerInRoom playerInRoom = new PlayerInRoom(npgsqlData.GetString(0), npgsqlData.GetString(1), npgsqlData.GetString(2));
            return playerInRoom;
        }
        private void AddWithValue(NpgsqlCommand cmd, PlayerInRoom playerInRoom)
        {
            cmd.Parameters.AddWithValue("roomId", playerInRoom.RoomId);
            cmd.Parameters.AddWithValue("id", playerInRoom.Id);
            cmd.Parameters.AddWithValue("name", playerInRoom.Name);
        }
    }
}
