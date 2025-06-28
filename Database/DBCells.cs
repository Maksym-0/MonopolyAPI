using Npgsql;
using Monopoly;
using Monopoly.Models;
using Monopoly.Abstractions;

namespace Monopoly.Database
{
    public class DBCells : ICellRepository
    {
        NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
        public async Task InsertCellsAsync(List<Cell> cells)
        {
            var sql = $"INSERT INTO PUBLIC.\"{Constants.DBcellName}\" (\"GameId\", \"Name\", \"Number\", \"Price\", \"Rent\", \"OwnerId\", \"Level\")" +
                "VALUES (@GameId, @Name, @Number, @Price, @Rent, @OwnerId, @Level)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);
            
            await _connection.OpenAsync();
            for(int i = 0; i < cells.Count; i++)
            {
                cmd.Parameters.AddWithValue("GameId", cells[i].GameId);
                cmd.Parameters.AddWithValue("Name", cells[i].Name);
                cmd.Parameters.AddWithValue("Number", cells[i].Number);
                if(cells[i].Price == null) cmd.Parameters.AddWithValue("Price", DBNull.Value);
                else cmd.Parameters.AddWithValue("Price", cells[i].Price);
                if (cells[i].Rent == null) cmd.Parameters.AddWithValue("Rent", DBNull.Value);
                else cmd.Parameters.AddWithValue("Rent", cells[i].Rent);
                if (cells[i].OwnerName == null) cmd.Parameters.AddWithValue("OwnerId", DBNull.Value);
                else cmd.Parameters.AddWithValue("OwnerId", cells[i].OwnerName);
                cmd.Parameters.AddWithValue("Level", cells[i].Level);

                await cmd.ExecuteNonQueryAsync();
                cmd.Parameters.Clear();
            }
            await _connection.CloseAsync();
        }
        public async Task<List<Cell>> ReadCellListAsync(string gameId)
        {
            List<Cell> cells = new List<Cell>();

            var sql = $"SELECT \"GameId\", \"Name\", \"Number\", \"Price\", \"Rent\", \"OwnerId\", \"Level\" " +
                $"FROM public.\"{Constants.DBcellName}\" " +
                $"WHERE \"GameId\" = @GameId";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("GameId", gameId);
            await _connection.OpenAsync();
            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();

            while (await npgsqlData.ReadAsync())
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
                        OwnerName = npgsqlData.GetString(5),
                        Level = npgsqlData.GetInt32(6),
                    };
                }
                cells.Add(cell);
            };
            await _connection.CloseAsync();
            return cells;
        }
        public async Task<Cell> ReadCellAsync(string gameId, int cellNumber)
        {
            List<Cell> cells = new List<Cell>();

            var sql = $"SELECT \"GameId\", \"Name\", \"Number\", \"Price\", \"Rent\", \"OwnerId\", \"Level\" " +
                $"FROM public.\"{Constants.DBcellName}\" " +
                $"WHERE \"GameId\" = @GameId AND \"Number\" = @Number";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("GameId", gameId);
            cmd.Parameters.AddWithValue("Number", cellNumber);

            await _connection.OpenAsync();
            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();

            Cell cell;
            await npgsqlData.ReadAsync();
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
                    OwnerName = npgsqlData.GetString(5),
                    Level = npgsqlData.GetInt32(6),
                };
            };
            await _connection.CloseAsync();
            return cell;
        }
        public async Task UpdateCellAsync(Cell cell)
        {
            var sql = $"UPDATE PUBLIC.\"{Constants.DBcellName}\" " +
                $"SET \"Name\" = @Name, \"Price\" = @Price, \"Rent\" = @Rent, \"OwnerId\" = @OwnerId, \"Level\" = @Level " +
                $"WHERE \"Number\" = @Number AND \"GameId\" = @GameId";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("GameId", cell.GameId);
            cmd.Parameters.AddWithValue("Name", cell.Name);
            cmd.Parameters.AddWithValue("Number", cell.Number); 
            if (cell.Price == null) cmd.Parameters.AddWithValue("Price", DBNull.Value);
            else cmd.Parameters.AddWithValue("Price", cell.Price);
            if (cell.Rent == null) cmd.Parameters.AddWithValue("Rent", DBNull.Value);
            else cmd.Parameters.AddWithValue("Rent", cell.Rent);
            if (cell.OwnerName == null) cmd.Parameters.AddWithValue("OwnerId", DBNull.Value);
            else cmd.Parameters.AddWithValue("OwnerId", cell.OwnerName);
            cmd.Parameters.AddWithValue("Level", cell.Level);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task DeleteCellsAsync(string gameId)
        {
            var sql = $"DELETE FROM PUBLIC.\"{Constants.DBcellName}\" " +
                $"WHERE \"GameId\" = @GameId";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("GameId", gameId);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
    }
}
