using Npgsql;
using Monopoly.Interfaces.IDatabases;
using Monopoly.Models.GameModels;

namespace Monopoly.Database
{
    public class DbPlayer : IPlayerRepository
    {
        public async Task InsertPlayersAsync(List<Player> players)
        {
            var sql = $"INSERT INTO PUBLIC.\"{Constants.DBplayerName}\" (\"Id\", \"Name\", \"GameId\", \"Balance\", \"Location\", \"CantAction\", \"ReverseMove\", \"LastDice1\", \"LastDice2\", \"CountOfDubles\", \"IsPrisoner\", \"InGame\", \"NeedPay\", \"HisAction\", \"CanMove\", \"CanBuyCell\", \"CanLevelUpCell\") " +
                "VALUES (@id, @name, @gameId, @balance, @location, @cantAction, @reverseMove, @lastDice1, @lastDice2, @countOfDubles, @isPrisoner, @inGame, @needPay, @hisAction, @canMove, @canBuyCell, @canLevelUpCell)";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
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
            var sql = $"SELECT \"Id\", \"Name\", \"GameId\", \"Balance\", \"Location\", \"CantAction\", \"ReverseMove\", \"LastDice1\", \"LastDice2\", \"CountOfDubles\", \"IsPrisoner\", \"InGame\", \"NeedPay\", \"HisAction\", \"CanMove\", \"CanBuyCell\", \"CanLevelUpCell\" " +
                $"FROM public.\"{Constants.DBplayerName}\" " +
                $"WHERE \"GameId\" = @gameId " +
                $"ORDER BY \"Id\"";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            List<Player> players = new List<Player>();
            cmd.Parameters.AddWithValue("gameId", gameId);
            await _connection.OpenAsync();
            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();
            
            while (await npgsqlData.ReadAsync())
            {
                Player player = ConstructPlayer(npgsqlData);
                players.Add(player);
            }
            await _connection.CloseAsync();
            if (players.Count == 0)
                throw new Exception("Гравців не знайдено");

            return players;
        }
        public async Task<Player> ReadPlayerAsync(string gameId, string playerId)
        {
            var sql = $"SELECT \"Id\", \"Name\", \"GameId\", \"Balance\", \"Location\", \"CantAction\", \"ReverseMove\", \"LastDice1\", \"LastDice2\", \"CountOfDubles\", \"IsPrisoner\", \"InGame\", \"NeedPay\", \"HisAction\", \"CanMove\", \"CanBuyCell\", \"CanLevelUpCell\" " +
                $"FROM public.\"{Constants.DBplayerName}\" " +
                $"WHERE \"GameId\" = @gameId AND \"Id\" = @id";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("gameId", gameId);
            cmd.Parameters.AddWithValue("id", playerId);
            
            await _connection.OpenAsync();
            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();
            
            if(!await npgsqlData.ReadAsync())
            {
                await _connection.CloseAsync();
                throw new Exception("Гравця не знайдено"); 
            }
            
            Player player = ConstructPlayer(npgsqlData);
            await _connection.CloseAsync();
            return player;
        }
        public async Task UpdatePlayerAsync(Player player)
        {
            var sql = $"UPDATE PUBLIC.\"{Constants.DBplayerName}\" " +
                $"SET \"Balance\" = @balance, \"Location\" = @location, \"CantAction\" = @cantAction, \"LastDice1\" = @lastDice1, \"LastDice2\" = @lastDice2, \"CountOfDubles\" = @countOfDubles, \"IsPrisoner\" = @isPrisoner, \"InGame\" = @inGame, \"NeedPay\" = @needPay, \"HisAction\" = @hisAction, \"CanMove\" = @canMove, \"CanBuyCell\" = @canBuyCell, \"CanLevelUpCell\" = @canLevelUpCell, \"ReverseMove\" = @reverseMove " +
                "WHERE \"GameId\" = @gameId AND \"Id\" = @id";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            AddWithValue(cmd, player);
            
            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task DeletePlayersAsync(string gameId)
        {
            var sql = $"DELETE FROM PUBLIC.\"{Constants.DBplayerName}\" " +
                "WHERE \"GameId\" = @gameId";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("gameId", gameId);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }

        private Player ConstructPlayer(NpgsqlDataReader npgsqlData)
        {
            Player player = new Player
            {
                Id = npgsqlData.GetString(0),
                Name = npgsqlData.GetString(1),
                GameId = npgsqlData.GetString(2),
                Balance = npgsqlData.GetInt32(3),
                Location = npgsqlData.GetInt32(4),
                CantAction = npgsqlData.GetInt32(5),
                ReverseMove = npgsqlData.GetInt32(6),
                LastDiceResult = new Dice(npgsqlData.GetInt32(7), npgsqlData.GetInt32(8)),
                CountOfDubles = npgsqlData.GetInt32(9),
                IsPrisoner = npgsqlData.GetBoolean(10),
                InGame = npgsqlData.GetBoolean(11),
                NeedPay = npgsqlData.GetBoolean(12),
                HisAction = npgsqlData.GetBoolean(13),
                CanRollDice = npgsqlData.GetBoolean(14),
                CanBuyCell = npgsqlData.GetBoolean(15),
                CanLevelUpCell = npgsqlData.GetBoolean(16)
            };
            return player;
        }
        private void AddWithValue(NpgsqlCommand cmd, Player player)
        {
            cmd.Parameters.AddWithValue("id", player.Id);
            cmd.Parameters.AddWithValue("name", player.Name);
            cmd.Parameters.AddWithValue("gameId", player.GameId);
            cmd.Parameters.AddWithValue("balance", player.Balance);
            cmd.Parameters.AddWithValue("location", player.Location);
            cmd.Parameters.AddWithValue("cantAction", player.CantAction);
            cmd.Parameters.AddWithValue("reverseMove", player.ReverseMove);
            cmd.Parameters.AddWithValue("lastDice1", player.LastDiceResult.Dice1);
            cmd.Parameters.AddWithValue("lastDice2", player.LastDiceResult.Dice2);
            cmd.Parameters.AddWithValue("countOfDubles", player.CountOfDubles);
            cmd.Parameters.AddWithValue("isPrisoner", player.IsPrisoner);
            cmd.Parameters.AddWithValue("inGame", player.InGame);
            cmd.Parameters.AddWithValue("needPay", player.NeedPay);
            cmd.Parameters.AddWithValue("hisAction", player.HisAction);
            cmd.Parameters.AddWithValue("canMove", player.CanRollDice);
            cmd.Parameters.AddWithValue("canBuyCell", player.CanBuyCell);
            cmd.Parameters.AddWithValue("canLevelUpCell", player.CanLevelUpCell);
        }
    }
}
