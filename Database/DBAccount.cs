using Npgsql;
using Monopoly.Interfaces.IDatabases;
using Monopoly.Models.AcountModels;

namespace Monopoly.Database
{
    public class DBAccount : IAccountRepository
    {
        private readonly NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
        
        public async Task InsertAccountAsync(Account acc)
        {
            var sql = $"INSERT INTO PUBLIC.\"{Constants.DBaccountName}\" (\"Id\", \"Name\", \"Password\") " +
                "VALUES (@id, @name, @password)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            AddWithValue(cmd, acc);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task<List<Account>> ReadAccountListAsync()
        {
            var sql = "SELECT \"Id\", \"Name\", \"Password\" " +
                $"FROM PUBLIC.\"{Constants.DBaccountName}\"";

            await _connection.OpenAsync();
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();

            Account acc;
            List<Account> accounts = new List<Account>();
            while(await npgsqlData.ReadAsync())
            {
                acc = ConstructAccount(npgsqlData);
                accounts.Add(acc);
            }

            await _connection.CloseAsync();
            return accounts;
        }
        public async Task<Account> ReadAccountAsync(string id)
        {
            var sql = "SELECT \"Id\", \"Name\", \"Password\" " +
                $"FROM PUBLIC.\"{Constants.DBaccountName}\" " +
                $"WHERE \"Id\" = @id";

            await _connection.OpenAsync();
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);
            cmd.Parameters.AddWithValue("id", id);

            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();
            if (!await npgsqlData.ReadAsync())
            {
                await _connection.CloseAsync();
                throw new Exception("Аккаунт не знайдено");
            }
            Account account = ConstructAccount(npgsqlData);

            await _connection.CloseAsync();
            return account;
        }
        public async Task UpdateAccountAsync(Account acc)
        {
            var sql = $"UPDATE PUBLIC.\"{Constants.DBaccountName}\" " +
                "SET \"Name\" = @name, \"Password\" = @password " +
                "WHERE \"Id\" = @id";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            AddWithValue(cmd, acc);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task DeleteAccountAsync(Account acc)
        {
            var sql = $"DELETE FROM PUBLIC.\"{Constants.DBaccountName}\" " +
                "WHERE \"Id\" = @id";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("id", acc.Id);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }

        public async Task<bool> SearchUserWithNameAsync(string name)
        {
            var sql = $"SELECT * FROM public.\"{Constants.DBaccountName}\" where \"Name\" = @name";

            using NpgsqlCommand cmd = new(sql, _connection);
            cmd.Parameters.AddWithValue("name", name);

            await _connection.OpenAsync();
            using NpgsqlDataReader sqlData = await cmd.ExecuteReaderAsync();
            bool data = await sqlData.ReadAsync();
            await _connection.CloseAsync();

            return data;
        }
        public async Task<bool> SearchUserWithIdAsync(string id)
        {
            var sql = $"SELECT * FROM public.\"{Constants.DBaccountName}\" where \"Id\" = @id";

            using NpgsqlCommand cmd = new(sql, _connection);
            cmd.Parameters.AddWithValue("id", id);

            await _connection.OpenAsync();
            using NpgsqlDataReader sqlData = await cmd.ExecuteReaderAsync();

            bool data = await sqlData.ReadAsync();
            await _connection.CloseAsync();

            return data;
        }

        private Account ConstructAccount(NpgsqlDataReader npgsqlData)
        {
            Account acc = new Account()
            {
                Id = npgsqlData.GetString(0),
                Name = npgsqlData.GetString(1),
                Password = npgsqlData.GetString(2)
            };
            return acc;
        }
        private void AddWithValue(NpgsqlCommand cmd, Account acc)
        {
            cmd.Parameters.AddWithValue("id", acc.Id);
            cmd.Parameters.AddWithValue("name", acc.Name);
            cmd.Parameters.AddWithValue("password", acc.Password);
        }
    }
}
