using Monopoly.Models;

namespace Monopoly.Abstractions
{
    public interface IAccountRepository
    {
        Task InsertAccountAsync(Account acc);
        Task<List<Account>> ReadAccountListAsync();
        Task<Account> ReadAccountAsync(string name);
        Task UpdateAccountAsync(Account acc);
        Task DeleteAccountAsync(Account acc);
    }
}
