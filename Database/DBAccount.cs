using Npgsql;
using Monopoly.Interfaces.IDatabases;
using Monopoly.Models.AccountModels;

namespace Monopoly.Database
{
    public class DbAccount : IAccountRepository
    {
        public async Task InsertAccountAsync(Account acc)
        {
            var sql = $"INSERT INTO PUBLIC.\"{Constants.DBaccountName}\" (\"Id\", \"Name\", \"Password\") " +
                "VALUES (@id, @name, @password)";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
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
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);
            await _connection.OpenAsync();
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
        public async Task<Account> ReadAccountWithIdAsync(string id)
        {
            var sql = "SELECT \"Id\", \"Name\", \"Password\" " +
                $"FROM PUBLIC.\"{Constants.DBaccountName}\" " +
                $"WHERE \"Id\" = @id";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);
            await _connection.OpenAsync();
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
        public async Task<Account> ReadAccountWithNameAsync(string name)
        {
            var sql = $"SELECT \"Id\", \"Name\", \"Password\" " +
                $"FROM PUBLIC.\"{Constants.DBaccountName}\" " +
                $"WHERE \"Name\" = @name";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);
            await _connection.OpenAsync();
            cmd.Parameters.AddWithValue("name", name);

            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();
            if(!await npgsqlData.ReadAsync())
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
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
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
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("id", acc.Id);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }

        public async Task<bool> SearchUserWithNameAsync(string name)
        {
            var sql = $"SELECT \"Id\", \"Name\", \"Password\" " +
                $"FROM public.\"{Constants.DBaccountName}\" " +
                $"WHERE \"Name\" = @name";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new(sql, _connection);
            cmd.Parameters.AddWithValue("name", name);

            await _connection.OpenAsync();
            NpgsqlDataReader sqlData = await cmd.ExecuteReaderAsync();
            bool data = await sqlData.ReadAsync();
            await _connection.CloseAsync();

            return data;
        }
        public async Task<bool> SearchUserWithIdAsync(string id)
        {
            var sql = $"SELECT \"Id\", \"Name\", \"Password\" " +
                $"FROM public.\"{Constants.DBaccountName}\" " +
                $"WHERE \"Id\" = @id";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
            NpgsqlCommand cmd = new(sql, _connection);
            cmd.Parameters.AddWithValue("id", id);

            await _connection.OpenAsync();
            NpgsqlDataReader sqlData = await cmd.ExecuteReaderAsync();

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
