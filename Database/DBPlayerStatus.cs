using Npgsql;
using Monopoly.Models;
using Monopoly.Abstractions;

namespace Monopoly.Database
{
    public class DBPlayerStatus : IPlayerRepository
    {
        NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
        public async Task InsertPlayersAsync(List<Player> players)
        {
            var sql = $"INSERT INTO PUBLIC.\"{Constants.DBplayerName}\" (\"Name\", \"GameId\", \"Balance\", \"Location\", \"CantAction\", \"IsPrisoner\", \"IsPaid\", \"InGame\", \"HisAction\", \"CanMove\", \"CanByCell\", \"CanLevelUp\", \"ReverseMove\") " +
                "VALUES (@Name, @GameId, @Balance, @Location, @IsPrisoner, @IsPaid, @InGame, @CanMove, @ReverseMove)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            await _connection.OpenAsync();
            for(int i =  0; i < players.Count; i++)
            {
                cmd.Parameters.AddWithValue("GameId", players[i].GameId);
                cmd.Parameters.AddWithValue("Name", players[i].Name);
                cmd.Parameters.AddWithValue("Balance", players[i].Balance);
                cmd.Parameters.AddWithValue("Location", players[i].Location);
                cmd.Parameters.AddWithValue("CantAction", players[i].CantAction);
                cmd.Parameters.AddWithValue("IsPrisoner", players[i].IsPrisoner);
                cmd.Parameters.AddWithValue("InGame", players[i].InGame);
                cmd.Parameters.AddWithValue("IsPaid", players[i].NeedPay);
                cmd.Parameters.AddWithValue("HisAction", players[i].HisAction);
                cmd.Parameters.AddWithValue("CanMove", players[i].CanMove);
                cmd.Parameters.AddWithValue("CanByCell", players[i].CanBuyCell);
                cmd.Parameters.AddWithValue("CanLevelUp", players[i].CanUpdateCell);
                cmd.Parameters.AddWithValue("ReverseMove", players[i].ReverseMove);

                await cmd.ExecuteNonQueryAsync();
                cmd.Parameters.Clear();
            }
            await _connection.CloseAsync();
        }
        public async Task<List<Player>> ReadPlayerListAsync(string gameId)
        {
            var sql = $"SELECT \"Name\", \"GameId\", \"Balance\", \"Location\", \"CantAction\", \"IsPrisoner\", \"IsPaid\", \"InGame\", \"HisAction\", \"CanMove\", \"CanByCell\", \"CanLevelUp\", \"ReverseMove\" " +
                $"FROM public.\"{Constants.DBplayerName}\" " +
                $"WHERE \"GameId\" = @GameId";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            List<Player> players = new List<Player>();
            cmd.Parameters.AddWithValue("GameId", gameId);
            await _connection.OpenAsync();
            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();

            while (await npgsqlData.ReadAsync())
            {
                Player player = new Player
                {
                    Name = npgsqlData.GetString(0),
                    GameId = npgsqlData.GetString(1),
                    Balance = npgsqlData.GetInt32(2),
                    Location = npgsqlData.GetInt32(3),
                    CantAction = npgsqlData.GetInt32(4),
                    IsPrisoner = npgsqlData.GetBoolean(5),
                    NeedPay = npgsqlData.GetBoolean(6),
                    InGame = npgsqlData.GetBoolean(7),
                    HisAction = npgsqlData.GetBoolean(8),
                    CanMove = npgsqlData.GetBoolean(9),
                    CanBuyCell = npgsqlData.GetBoolean(10),
                    CanUpdateCell = npgsqlData.GetBoolean(11),
                    ReverseMove = npgsqlData.GetInt32(12)
                };

                players.Add(player);
            }
            await _connection.CloseAsync();
            return players;
        }
        public async Task<Player> ReadPlayerAsync(string gameId, string name)
        {
            var sql = $"SELECT \"Name\", \"GameId\", \"Balance\", \"Location\", \"CantAction\", \"IsPrisoner\", \"IsPaid\", \"InGame\", \"HisAction\", \"CanMove\", \"CanByCell\", \"CanLevelUp\", \"ReverseMove\" " +
                $"FROM public.\"{Constants.DBplayerName}\" " +
                $"WHERE \"GameId\" = @GameId AND \"Name\" = @Name";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("GameId", gameId);
            cmd.Parameters.AddWithValue("Name", name);
            
            await _connection.OpenAsync();
            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();
            await npgsqlData.ReadAsync();

            Player player = new Player()
            {
                Name = npgsqlData.GetString(0),
                GameId = npgsqlData.GetString(1),
                Balance = npgsqlData.GetInt32(2),
                Location = npgsqlData.GetInt32(3),
                CantAction = npgsqlData.GetInt32(4),
                IsPrisoner = npgsqlData.GetBoolean(5),
                NeedPay = npgsqlData.GetBoolean(6),
                InGame = npgsqlData.GetBoolean(7),
                HisAction = npgsqlData.GetBoolean(8),
                CanMove = npgsqlData.GetBoolean(9),
                CanBuyCell = npgsqlData.GetBoolean(10),
                CanUpdateCell = npgsqlData.GetBoolean(11),
                ReverseMove = npgsqlData.GetInt32(12)
            };
            await _connection.CloseAsync();
            return player;
        }
        public async Task UpdatePlayerAsync(Player player)
        {
            var sql = $"UPDATE PUBLIC.\"{Constants.DBplayerName}\" " +
                $"SET \"Balance\" = @Balance, \"Location\" = @Location, \"CantAction\" = @CantAction, \"IsPrisoner\" = @IsPrisoner, \"InGame\" = @InGame, \"IsPaid\" = @IsPaid, \"HisAction\" = @HisAction, \"CanMove\" = @CanMove, \"CanByCell\" = @CanByCell, \"CanLevelUp\" = @CanLevelUp, \"ReverseMove\" = @ReverseMove " +
                "WHERE \"GameId\" = @GameId AND \"Name\" = @Name";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("GameId", player.GameId);
            cmd.Parameters.AddWithValue("Name", player.Name);
            cmd.Parameters.AddWithValue("Balance", player.Balance);
            cmd.Parameters.AddWithValue("Location", player.Location);
            cmd.Parameters.AddWithValue("CantAction", player.CantAction);
            cmd.Parameters.AddWithValue("IsPrisoner", player.IsPrisoner);
            cmd.Parameters.AddWithValue("InGame", player.InGame);
            cmd.Parameters.AddWithValue("IsPaid", player.NeedPay);
            cmd.Parameters.AddWithValue("HisAction", player.HisAction);
            cmd.Parameters.AddWithValue("CanMove", player.CanMove);
            cmd.Parameters.AddWithValue("CanByCell", player.CanBuyCell);
            cmd.Parameters.AddWithValue("CanLevelUp", player.CanUpdateCell);
            cmd.Parameters.AddWithValue("ReverseMove", player.ReverseMove);
            
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
    }
}
