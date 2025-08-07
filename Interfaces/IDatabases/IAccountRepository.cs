using Monopoly.Models.AcountModels;
using Npgsql;
using System.Data.Common;

namespace Monopoly.Interfaces.IDatabases
{
    public interface IAccountRepository
    {
        Task InsertAccountAsync(Account acc);
        Task<List<Account>> ReadAccountListAsync();
        Task<Account> ReadAccountWithIdAsync(string name);
        Task<Account> ReadAccountWithNameAsync(string name);
        Task UpdateAccountAsync(Account acc);
        Task DeleteAccountAsync(Account acc);

        Task<bool> SearchUserWithNameAsync(string name);
        Task<bool> SearchUserWithIdAsync(string id);
    }
}
