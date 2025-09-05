using Monopoly.Models.AccountModels;

namespace Monopoly.Models.ApiResponse
{
    public class AccountDto
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public AccountDto(Account account)
        {
            Id = account.Id;
            Name = account.Name;
        }
        public AccountDto() { }
    }
}
