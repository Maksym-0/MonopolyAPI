using Npgsql;
using Monopoly.Models;
using Monopoly.Abstractions;
using System.Numerics;

namespace Monopoly.Database
{
    public class DBPlayerStatus : IPlayerRepository
    {
        NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
        public async Task InsertPlayersAsync(List<Player> players)
        {
            var sql = $"INSERT INTO PUBLIC.\"{Constants.DBplayerName}\" (\"Name\", \"GameId\", \"Balance\", \"Location\", \"CantAction\", \"ReverseMove\", \"IsPrisoner\", \"InGame\", \"NeedPay\", \"HisAction\", \"CanMove\", \"CanBuyCell\", \"CanLevelUpCell\") " +
                "VALUES (@Name, @GameId, @Balance, @Location, @CantAction, @ReverseMove, @IsPrisoner, @InGame, @NeedPay, @HisAction, @CanMove, @CanBuyCell, @CanLevelUpCell)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            await _connection.OpenAsync();
            for(int i =  0; i < players.Count; i++)
            {
                AddWithValue(cmd, players[i]);

                await cmd.ExecuteNonQueryAsync();
                cmd.Parameters.Clear();
            }
            await _connection.CloseAsync();
        }
        public async Task<List<Player>> ReadPlayerListAsync(string gameId)
        {
            var sql = $"SELECT \"Name\", \"GameId\", \"Balance\", \"Location\", \"CantAction\", \"ReverseMove\", \"IsPrisoner\", \"InGame\", \"NeedPay\", \"HisAction\", \"CanMove\", \"CanBuyCell\", \"CanLevelUpCell\" " +
                $"FROM public.\"{Constants.DBplayerName}\" " +
                $"WHERE \"GameId\" = @GameId";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            List<Player> players = new List<Player>();
            cmd.Parameters.AddWithValue("GameId", gameId);
            await _connection.OpenAsync();
            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();

            while (await npgsqlData.ReadAsync())
            {
                Player player = ConstructPlayer(npgsqlData);
                players.Add(player);
            }
            await _connection.CloseAsync();
            return players;
        }
        public async Task<Player> ReadPlayerAsync(string gameId, string name)
        {
            var sql = $"SELECT \"Name\", \"GameId\", \"Balance\", \"Location\", \"CantAction\", \"ReverseMove\", \"IsPrisoner\", \"InGame\", \"NeedPay\", \"HisAction\", \"CanMove\", \"CanBuyCell\", \"CanLevelUpCell\" " +
                $"FROM public.\"{Constants.DBplayerName}\" " +
                $"WHERE \"GameId\" = @GameId AND \"Name\" = @Name";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("GameId", gameId);
            cmd.Parameters.AddWithValue("Name", name);
            
            await _connection.OpenAsync();
            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();
            await npgsqlData.ReadAsync();

            Player player = ConstructPlayer(npgsqlData);
            await _connection.CloseAsync();
            return player;
        }
        public async Task UpdatePlayerAsync(Player player)
        {
            var sql = $"UPDATE PUBLIC.\"{Constants.DBplayerName}\" " +
                $"SET \"Balance\" = @Balance, \"Location\" = @Location, \"CantAction\" = @CantAction, \"IsPrisoner\" = @IsPrisoner, \"InGame\" = @InGame, \"NeedPay\" = @NeedPay, \"HisAction\" = @HisAction, \"CanMove\" = @CanMove, \"CanBuyCell\" = @CanBuyCell, \"CanLevelUpCell\" = @CanLevelUpCell, \"ReverseMove\" = @ReverseMove " +
                "WHERE \"GameId\" = @GameId AND \"Name\" = @Name";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            AddWithValue(cmd, player);
            
            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task DeletePlayersAsync(string gameId)
        {
            var sql = $"DELETE FROM PUBLIC.\"{Constants.DBplayerName}\" " +
                "WHERE \"GameId\" = @GameId";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("GameId", gameId);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }

        private Player ConstructPlayer(NpgsqlDataReader npgsqlData)
        {
            Player player = new Player
            {
                Name = npgsqlData.GetString(0),
                GameId = npgsqlData.GetString(1),
                Balance = npgsqlData.GetInt32(2),
                Location = npgsqlData.GetInt32(3),
                CantAction = npgsqlData.GetInt32(4),
                ReverseMove = npgsqlData.GetInt32(5),
                IsPrisoner = npgsqlData.GetBoolean(6),
                InGame = npgsqlData.GetBoolean(7),
                NeedPay = npgsqlData.GetBoolean(8),
                HisAction = npgsqlData.GetBoolean(9),
                CanMove = npgsqlData.GetBoolean(10),
                CanBuyCell = npgsqlData.GetBoolean(11),
                CanLevelUpCell = npgsqlData.GetBoolean(12)
            };
            return player;
        }
        private void AddWithValue(NpgsqlCommand cmd, Player player)
        {
            cmd.Parameters.AddWithValue("Name", player.Name);
            cmd.Parameters.AddWithValue("GameId", player.GameId);
            cmd.Parameters.AddWithValue("Balance", player.Balance);
            cmd.Parameters.AddWithValue("Location", player.Location);
            cmd.Parameters.AddWithValue("CantAction", player.CantAction);
            cmd.Parameters.AddWithValue("ReverseMove", player.ReverseMove);
            cmd.Parameters.AddWithValue("IsPrisoner", player.IsPrisoner);
            cmd.Parameters.AddWithValue("InGame", player.InGame);
            cmd.Parameters.AddWithValue("NeedPay", player.NeedPay);
            cmd.Parameters.AddWithValue("HisAction", player.HisAction);
            cmd.Parameters.AddWithValue("CanMove", player.CanMove);
            cmd.Parameters.AddWithValue("CanBuyCell", player.CanBuyCell);
            cmd.Parameters.AddWithValue("CanLevelUpCell", player.CanLevelUpCell);
        }
    }
}
