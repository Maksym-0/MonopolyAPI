using Monopoly.Database;
using Monopoly.Models;

namespace Monopoly.Interfaces.IServices
{
    public interface IAccountService
    {
        Task<bool> TryRegisterAsync(string name, string password);
        Task<string?> TryLoginAsync(string name, string password);
        Task<bool> TryDeleteAsync(string name, string password);
    }
}
