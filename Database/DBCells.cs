using Npgsql;
using Monopoly.Interfaces.IDatabases;
using Monopoly.Models.GameModels;

namespace Monopoly.Database
{
    public class DBCells : ICellRepository
    {
        private readonly NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
        
        public async Task InsertCellsAsync(List<Cell> cells)
        {
            var sql = $"INSERT INTO PUBLIC.\"{Constants.DBcellName}\" (\"GameId\", \"Name\", \"Number\", \"Price\", \"Rent\", \"OwnerName\", \"Level\")" +
                "VALUES (@gameId, @name, @number, @price, @rent, @ownerName, @level)";
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

            var sql = $"SELECT \"GameId\", \"Name\", \"Number\", \"Price\", \"Rent\", \"OwnerName\", \"Level\" " +
                $"FROM public.\"{Constants.DBcellName}\" " +
                $"WHERE \"GameId\" = @gameId";
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

            var sql = $"SELECT \"GameId\", \"Name\", \"Number\", \"Price\", \"Rent\", \"OwnerName\", \"Level\" " +
                $"FROM public.\"{Constants.DBcellName}\" " +
                $"WHERE \"GameId\" = @gameId AND \"number\" = @Number";
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
                $"SET \"Name\" = @name, \"Price\" = @price, \"Rent\" = @rent, \"OwnerName\" = @ownerName, \"Level\" = @level " +
                $"WHERE \"Number\" = @number AND \"GameId\" = @gameId";
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
                    Name = npgsqlData.GetString(1),
                    Number = npgsqlData.GetInt32(2),
                    Price = npgsqlData.GetInt32(3),
                    Rent = npgsqlData.GetInt32(4),
                    Owner = npgsqlData.GetString(5),
                    Level = npgsqlData.GetInt32(6),
                };
            }
            return cell;
        }
        private void AddWithValue(NpgsqlCommand cmd, Cell cell)
        {
            cmd.Parameters.AddWithValue("gameId", cell.GameId);
            cmd.Parameters.AddWithValue("name", cell.Name);
            cmd.Parameters.AddWithValue("number", cell.Number);
            if (cell.Price == null) cmd.Parameters.AddWithValue("price", DBNull.Value);
            else cmd.Parameters.AddWithValue("price", cell.Price);
            if (cell.Rent == null) cmd.Parameters.AddWithValue("rent", DBNull.Value);
            else cmd.Parameters.AddWithValue("rent", cell.Rent);
            if (cell.Owner == null) cmd.Parameters.AddWithValue("ownerName", DBNull.Value);
            else cmd.Parameters.AddWithValue("ownerName", cell.Owner);
            cmd.Parameters.AddWithValue("level", cell.Level);
        }
    }
}
