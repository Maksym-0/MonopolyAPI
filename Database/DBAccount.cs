using Npgsql;
using Monopoly;
using Monopoly.Models;
using Monopoly.Abstractions;
using System.Xml.Linq;
using System.Security.Principal;

namespace Monopoly.Database
{
    public class DBAccount : IAccountRepository
    {
        NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
        
        public async Task InsertAccountAsync(Account acc)
        {
            var sql = $"INSERT INTO PUBLIC.\"{Constants.DBaccountName}\" (\"ID\", \"Name\", \"Password\")" +
                "VALUES (@id, @name, @password)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            AddWithValue(cmd, acc);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task<List<Account>> ReadAccountListAsync()
        {
            var sql = "SELECT \"ID\", \"Name\", \"Password\" " +
                $"FROM public.\"{Constants.DBaccountName}\"";

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
        public async Task<Account> ReadAccountAsync(string name)
        {
            var sql = "SELECT \"ID\", \"Name\", \"Password\" " +
                $"FROM public.\"{Constants.DBaccountName}\" " +
                $"WHERE \"Name\" = @name";

            await _connection.OpenAsync();
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);
            cmd.Parameters.AddWithValue("name", name);

            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();
            await npgsqlData.ReadAsync();

            Account account = ConstructAccount(npgsqlData);

            await _connection.CloseAsync();
            return account;
        }
        public async Task UpdateAccountAsync(Account acc)
        {
            var sql = $"UPDATE PUBLIC.\"{Constants.DBaccountName}\" " +
                "SET \"Name\" = @Name, \"Password\" = @Password " +
                "WHERE \"ID\" = @id";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            AddWithValue(cmd, acc);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task DeleteAccountAsync(Account acc)
        {
            var sql = $"DELETE FROM public.\"{Constants.DBaccountName}\" " +
                "WHERE \"ID\" = @id";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("id", acc.ID);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }

        private Account ConstructAccount(NpgsqlDataReader npgsqlData)
        {
            Account acc = new Account()
            {
                ID = npgsqlData.GetString(0),
                Name = npgsqlData.GetString(1),
                Password = npgsqlData.GetString(2)
            };
            return acc;
        }
        private void AddWithValue(NpgsqlCommand cmd, Account acc)
        {
            cmd.Parameters.AddWithValue("id", acc.ID);
            cmd.Parameters.AddWithValue("name", acc.Name);
            cmd.Parameters.AddWithValue("password", acc.Password);
        }
    }
}
