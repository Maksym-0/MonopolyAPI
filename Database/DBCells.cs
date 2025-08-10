using Npgsql;
using Monopoly.Interfaces.IDatabases;
using Monopoly.Models.GameModels;

namespace Monopoly.Database
{
    public class DBCells : ICellRepository
    {
        public async Task InsertCellsAsync(List<Cell> cells)
        {
            var sql = $"INSERT INTO PUBLIC.\"{Constants.DBcellName}\" (\"GameId\", \"Unique\", \"Name\", \"Number\", \"Price\", \"Rent\", \"Owner\", \"Level\")" +
                "VALUES (@gameId, @name, @number, @price, @rent, @owner, @level)";
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

            var sql = $"SELECT \"GameId\", \"Unique\", \"Name\", \"Number\", \"Price\", \"Rent\", \"Owner\", \"Level\" " +
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

            var sql = $"SELECT \"GameId\", \"Unique\", \"Name\", \"Number\", \"Price\", \"Rent\", \"Owner\", \"Level\" " +
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
                $"SET \"Name\" = @name, \"Unique\" = @unique, \"Price\" = @price, \"Rent\" = @rent, \"Owner\" = @owner, \"Level\" = @level " +
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
            if (npgsqlData.IsDBNull(3))
            {
                cell = new Cell(npgsqlData.GetString(0), npgsqlData.GetString(1), npgsqlData.GetInt32(2));
            }
            else
            {
                cell = new Cell
                {
                    GameId = npgsqlData.GetString(0),
                    Unique = npgsqlData.GetBoolean(1),
                    Name = npgsqlData.GetString(2),
                    Number = npgsqlData.GetInt32(3),
                    Price = npgsqlData.GetInt32(4),
                    Rent = npgsqlData.GetInt32(5),
                    Owner = npgsqlData.GetString(6),
                    Level = npgsqlData.GetInt32(7),
                };
            }
            return cell;
        }
        private void AddWithValue(NpgsqlCommand cmd, Cell cell)
        {
            cmd.Parameters.AddWithValue("gameId", cell.GameId);
            cmd.Parameters.AddWithValue("unique", cell.Unique);
            cmd.Parameters.AddWithValue("name", cell.Name);
            cmd.Parameters.AddWithValue("number", cell.Number);
            if (cell.Price == null) cmd.Parameters.AddWithValue("price", DBNull.Value);
            else cmd.Parameters.AddWithValue("price", cell.Price);
            if (cell.Rent == null) cmd.Parameters.AddWithValue("rent", DBNull.Value);
            else cmd.Parameters.AddWithValue("rent", cell.Rent);
            if (cell.Owner == null) cmd.Parameters.AddWithValue("owner", DBNull.Value);
            else cmd.Parameters.AddWithValue("owner", cell.Owner);
            cmd.Parameters.AddWithValue("level", cell.Level);
        }
    }
}
