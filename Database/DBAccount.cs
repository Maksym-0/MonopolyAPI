using Npgsql;
using Monopoly;
using Monopoly.Models;
using Monopoly.Abstractions;
using System.Xml.Linq;

namespace Monopoly.Database
{
    public class DBAccount : IAccountRepository
    {
        NpgsqlConnection _connection = new NpgsqlConnection(Constants.Connect);
        
        public async Task InsertAccountAsync(Account acc)
        {
            var sql = $"INSERT INTO PUBLIC.\"{Constants.DBaccountName}\" (\"Name\", \"Password\")" +
                "VALUES (@name, @password)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("name", acc.Name);
            cmd.Parameters.AddWithValue("password", acc.Password);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task<List<Account>> ReadAccountListAsync()
        {
            var sql = "SELECT \"Name\", \"Password\" " +
                $"FROM public.\"{Constants.DBaccountName}\"";

            await _connection.OpenAsync();
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();

            Account account = new Account();
            List<Account> accounts = new List<Account>();
            while(await npgsqlData.ReadAsync())
            {
                account.Name = npgsqlData.GetString(0);
                account.Password = npgsqlData.GetString(1);
                accounts.Add(account);
            }

            await _connection.CloseAsync();
            return accounts;
        }
        public async Task<Account> ReadAccountAsync(string name)
        {
            var sql = "SELECT \"Name\", \"Password\" " +
                $"FROM public.\"{Constants.DBaccountName}\" " +
                $"WHERE \"Name\" = @name";

            await _connection.OpenAsync();
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);
            cmd.Parameters.AddWithValue("name", name);

            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();
            await npgsqlData.ReadAsync();

            Account account = new Account();
            account.Name = npgsqlData.GetString(0);
            account.Password = npgsqlData.GetString(1);

            await _connection.CloseAsync();
            return account;
        }
        public async Task UpdateAccountAsync(Account acc)
        {
            var sql = "SET \"Name\" = @Name, \"Password\" = @Password " +
                $"FROM public.\"{Constants.DBaccountName}\"";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("Name", acc.Name);
            cmd.Parameters.AddWithValue("Password", acc.Password);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task DeleteAccountAsync(Account acc)
        {
            var sql = $"DELETE FROM public.\"{Constants.DBaccountName}\" " +
                "WHERE \"Name\" = @name AND \"Password\" = @password";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("name", acc.Name);
            cmd.Parameters.AddWithValue("password", acc.Password);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
    }
}
