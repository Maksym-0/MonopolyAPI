using Monopoly.Models.ApiResponse;

namespace Monopoly.Interfaces.IServices
{
    public interface IAccountService
    {
        Task<AccountDto> TryRegisterAsync(string name, string password);
        Task<LoginDto> TryLoginAsync(string name, string password);
        Task<DeleteAccountDto> TryDeleteAsync(string name, string password);
    }
}
