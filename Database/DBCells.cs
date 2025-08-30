using Npgsql;
using Monopoly.Interfaces.IDatabases;
using Monopoly.Models.GameModels;

namespace Monopoly.Database
{
    public class DbCells : ICellRepository
    {
        public async Task InsertCellsAsync(List<Cell> cells)
        {
            var sql = $"INSERT INTO PUBLIC.\"{Constants.DBcellName}\" (\"GameId\", \"MonopolyIndex\", \"MonopolyType\", \"Unique\", \"IsMonopoly\", \"Name\", \"Number\", \"Price\", \"Rent\", \"OwnerId\", \"Level\")" +
                "VALUES (@gameId, @monopolyIndex, @monopolyType, @unique, @isMonopoly, @name, @number, @price, @rent, @ownerId, @level)";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);
            
            await _connection.OpenAsync();
            for(int i = 0; i < cells.Count; i++)
            {
                AddWithValue(cmd, cells[i]);

                await cmd.ExecuteNonQueryAsync();
                cmd.Parameters.Clear();
            }
            await _connection.CloseAsync();
        }
        public async Task<List<Cell>> ReadCellListAsync(string gameId)
        {
            List<Cell> cells = new List<Cell>();

            var sql = $"SELECT \"GameId\", \"MonopolyIndex\", \"MonopolyType\", \"Unique\", \"IsMonopoly\", \"Name\", \"Number\", \"Price\", \"Rent\", \"OwnerId\", \"Level\" " +
                $"FROM public.\"{Constants.DBcellName}\" " +
                $"WHERE \"GameId\" = @gameId";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("gameId", gameId);
            await _connection.OpenAsync();
            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();
            
            while (await npgsqlData.ReadAsync())
            {
                Cell cell = ConstructCell(npgsqlData);
                cells.Add(cell);
            };
            await _connection.CloseAsync();
            if (cells.Count == 0)
                throw new Exception("Клітини не знайдено");

            return cells;
        }
        public async Task<Cell> ReadCellAsync(string gameId, int cellNumber)
        {
            List<Cell> cells = new List<Cell>();

            var sql = $"SELECT \"GameId\", \"MonopolyIndex\", \"MonopolyType\", \"Unique\", \"IsMonopoly\", \"Name\", \"Number\", \"Price\", \"Rent\", \"OwnerId\", \"Level\" " +
                $"FROM public.\"{Constants.DBcellName}\" " +
                $"WHERE \"GameId\" = @gameId AND \"Number\" = @number";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("gameId", gameId);
            cmd.Parameters.AddWithValue("number", cellNumber);

            await _connection.OpenAsync();
            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();
            
            if (!await npgsqlData.ReadAsync())
            {
                await _connection.CloseAsync();
                throw new Exception("Клітину не знайдено");
            }

            Cell cell = ConstructCell(npgsqlData);
            await _connection.CloseAsync();

            return cell;
        }
        public async Task UpdateCellAsync(Cell cell)
        {
            var sql = $"UPDATE PUBLIC.\"{Constants.DBcellName}\" " +
                $"SET \"IsMonopoly\" = @isMonopoly, \"Name\" = @name, \"Unique\" = @unique, \"Price\" = @price, \"Rent\" = @rent, \"OwnerId\" = @ownerId, \"Level\" = @level " +
                $"WHERE \"Number\" = @number AND \"GameId\" = @gameId";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);
            
            AddWithValue(cmd, cell);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task DeleteCellsAsync(string gameId)
        {
            var sql = $"DELETE FROM PUBLIC.\"{Constants.DBcellName}\" " +
                $"WHERE \"GameId\" = @gameId";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("gameId", gameId);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }

        private Cell ConstructCell(NpgsqlDataReader npgsqlData)
        {
            Cell cell;
            if (npgsqlData.GetBoolean(3))
            {
                cell = new Cell(npgsqlData.GetString(0), npgsqlData.GetString(5), npgsqlData.GetInt32(6));
            }
            else
            {
                cell = new Cell
                {
                    GameId = npgsqlData.GetString(0),
                    MonopolyIndex = npgsqlData.IsDBNull(1) ? null : npgsqlData.GetInt32(1),
                    MonopolyType = npgsqlData.IsDBNull(2) ? null : npgsqlData.GetString(2),
                    Unique = npgsqlData.GetBoolean(3),
                    IsMonopoly = npgsqlData.IsDBNull(4) ? null : npgsqlData.GetBoolean(4),
                    Name = npgsqlData.GetString(5),
                    Number = npgsqlData.GetInt32(6),
                    Price = npgsqlData.IsDBNull(7) ? null : npgsqlData.GetInt32(7),
                    Rent = npgsqlData.IsDBNull(8) ? null : npgsqlData.GetInt32(8),
                    OwnerId = npgsqlData.IsDBNull(9) ? null : npgsqlData.GetString(9),
                    Level = npgsqlData.GetInt32(10),
                };
            }
            return cell;
        }
        private void AddWithValue(NpgsqlCommand cmd, Cell cell)
        {
            cmd.Parameters.AddWithValue("gameId", cell.GameId);
            if (cell.MonopolyIndex == null ) cmd.Parameters.AddWithValue("monopolyIndex", DBNull.Value);
            else cmd.Parameters.AddWithValue("monopolyIndex", cell.MonopolyIndex);
            if (cell.MonopolyType == null) cmd.Parameters.AddWithValue("monopolyType", DBNull.Value);
            else cmd.Parameters.AddWithValue("monopolyType", cell.MonopolyType);
            cmd.Parameters.AddWithValue("unique", cell.Unique);
            if (cell.IsMonopoly == null) cmd.Parameters.AddWithValue("isMonopoly", DBNull.Value);
            else cmd.Parameters.AddWithValue("isMonopoly", cell.IsMonopoly);
            cmd.Parameters.AddWithValue("name", cell.Name);
            cmd.Parameters.AddWithValue("number", cell.Number);
            if (cell.Price == null) cmd.Parameters.AddWithValue("price", DBNull.Value);
            else cmd.Parameters.AddWithValue("price", cell.Price);
            if (cell.Rent == null) cmd.Parameters.AddWithValue("rent", DBNull.Value);
            else cmd.Parameters.AddWithValue("rent", cell.Rent);
            if (cell.OwnerId == null) cmd.Parameters.AddWithValue("ownerId", DBNull.Value);
            else cmd.Parameters.AddWithValue("ownerId", cell.OwnerId);
            cmd.Parameters.AddWithValue("level", cell.Level);
        }
    }
}
