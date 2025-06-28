using Npgsql;
using Monopoly;
using Monopoly.Models;
using NpgsqlTypes;
using System.Xml.Linq;
using Monopoly.Abstractions;

namespace Monopoly.Database
{
    public class DBRoom : IRoomRepository
    {
        NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
        public async Task InsertRoomAsync(Room room)
        {
            var sql = $"INSERT INTO PUBLIC.\"{Constants.DBroomName}\" (\"RoomId\", \"MaxNumberOfPlayers\", \"NumberOfPlayers\", \"PlayerNames\", \"Password\", \"InGame\")" +
                "VALUES (@RoomId, @MaxNumberOfPlayers, @NumberOfPlayers, @PlayerNames, @Password, @InGame)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("RoomId", room.RoomId);
            cmd.Parameters.AddWithValue("MaxNumberOfPlayers", room.MaxNumberOfPlayers);
            cmd.Parameters.AddWithValue("NumberOfPlayers", 0);
            if(room.PlayerNames == null)
                cmd.Parameters.AddWithValue("PlayerNames", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("PlayerNames", room.PlayerNames.ToArray());
            if(room.Password == null)
                cmd.Parameters.AddWithValue("Password", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("Password", room.Password);
            cmd.Parameters.AddWithValue("InGame", false);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task<List<Room>> ReadRoomListAsync()
        {
            var sql = $"SELECT \"RoomId\", \"MaxNumberOfPlayers\", \"NumberOfPlayers\", \"PlayerNames\", \"Password\", \"InGame\" " +
                $"FROM public.\"{Constants.DBroomName}\"";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            List<Room> rooms = new List<Room>();
            await _connection.OpenAsync();
            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();
            
            while (await npgsqlData.ReadAsync())
            {
                Room room;
                if (npgsqlData.IsDBNull(4))
                {
                    room = new Room
                    {
                        RoomId = npgsqlData.GetString(0),
                        MaxNumberOfPlayers = npgsqlData.GetInt32(1),
                        NumberOfPlayers = npgsqlData.GetInt32(2),
                        PlayerNames = new List<string>(npgsqlData.GetFieldValue<string[]>(3)),
                        Password = null,
                        InGame = npgsqlData.GetBoolean(5)
                    };
                }
                else
                {
                    room = new Room
                    {
                        RoomId = npgsqlData.GetString(0),
                        MaxNumberOfPlayers = npgsqlData.GetInt32(1),
                        NumberOfPlayers = npgsqlData.GetInt32(2),
                        PlayerNames = new List<string>(npgsqlData.GetFieldValue<string[]>(3)),
                        Password = npgsqlData.GetString(4),
                        InGame = npgsqlData.GetBoolean(5)
                    };
                }
                rooms.Add(room);
            }
            await _connection.CloseAsync();
            return rooms;
        }
        public async Task<Room> ReadRoomAsync(string roomId)
        {
            var sql = $"SELECT \"RoomId\", \"MaxNumberOfPlayers\", \"NumberOfPlayers\", \"PlayerNames\", \"Password\", \"InGame\" " +
                $"FROM public.\"{Constants.DBroomName}\" " +
                $"WHERE \"RoomId\" = @RoomId";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("RoomId", roomId);
            await _connection.OpenAsync();
            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();
            await npgsqlData.ReadAsync();
            Room room;
            if (npgsqlData.IsDBNull(4))
            {
                room = new Room
                {
                    RoomId = npgsqlData.GetString(0),
                    MaxNumberOfPlayers = npgsqlData.GetInt32(1),
                    NumberOfPlayers = npgsqlData.GetInt32(2),
                    PlayerNames = new List<string>(npgsqlData.GetFieldValue<string[]>(3)),
                    Password = null,
                    InGame = npgsqlData.GetBoolean(5)
                };
            }
            else
            {
                room = new Room
                {
                    RoomId = npgsqlData.GetString(0),
                    MaxNumberOfPlayers = npgsqlData.GetInt32(1),
                    NumberOfPlayers = npgsqlData.GetInt32(2),
                    PlayerNames = new List<string>(npgsqlData.GetFieldValue<string[]>(3)),
                    Password = npgsqlData.GetString(4),
                    InGame = npgsqlData.GetBoolean(5)
                };
            }

            await _connection.CloseAsync();
            return room;
        }
        public async Task UpdateRoomAsync(Room room)
        {
            var sql = $"UPDATE public.\"{Constants.DBroomName}\" " +
                "SET \"PlayerNames\" = @PlayerNames, \"NumberOfPlayers\" = @NumberOfPlayers, \"RoomId\" = @RoomId, \"MaxNumberOfPlayers\" = @MaxNumberOfPlayers, \"InGame\" = @InGame, \"Password\" = @Password " +
                $"WHERE \"RoomId\" = @RoomId AND (\"Password\" = @Password OR (@Password IS NULL AND \"Password\" IS NULL))";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("RoomId", room.RoomId);
            if (room.Password == null)
                cmd.Parameters.AddWithValue("Password", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("Password", room.Password);
            if (room.PlayerNames == null)
                cmd.Parameters.AddWithValue("PlayerNames", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("PlayerNames", room.PlayerNames.ToArray());
            cmd.Parameters.AddWithValue("NumberOfPlayers", room.PlayerNames.Count);
            cmd.Parameters.AddWithValue("MaxNumberOfPlayers", room.MaxNumberOfPlayers);
            cmd.Parameters.AddWithValue("InGame", room.InGame);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task DeleteRoomAsync(string roomId)
        {
            var sql = $"DELETE FROM public.\"{Constants.DBroomName}\" " +
                "WHERE \"RoomId\" = @RoomId";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("RoomId", roomId);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
    }
}
