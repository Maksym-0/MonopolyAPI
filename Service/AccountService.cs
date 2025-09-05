using Microsoft.IdentityModel.Tokens;
using Monopoly.Interfaces.IDatabases;
using Monopoly.Interfaces.IServices;
using Monopoly.Models.AccountModels;
using Monopoly.Models.ApiResponse;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Monopoly.Service
{
    public class AccountService : IAccountService
    {
        Random random = new Random();
        IAccountRepository dbAccount;

        public AccountService(IAccountRepository accountRepository) 
        {
            dbAccount = accountRepository;
        }

        public async Task<AccountDto> TryRegisterAsync(string name, string password)
        {
            if (await dbAccount.SearchUserWithNameAsync(name))
                throw new Exception("Акаунт із цим ім'ям вже зареєстровано");

            Account account = new Account(await GenerateIdAsync(12), name, password);
            AccountDto accountDto = new AccountDto(account);
            
            await dbAccount.InsertAccountAsync(account);
            
            return accountDto;
        }
        public async Task<LoginDto?> TryLoginAsync(string name, string password)
        {
            if (!await dbAccount.SearchUserWithNameAsync(name))
                throw new Exception("Користувача із цим ім'ям не знайдено");
            Account account = await dbAccount.ReadAccountWithNameAsync(name);

            if (password == account.Password)
            {
                string token = GenerateToken(account.Id, account.Name);
                return new LoginDto()
                {
                    Account = new AccountDto(account),
                    Token = token,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(12)
                };
            }
            return null;
        }
        public async Task<DeleteAccountDto> TryDeleteAsync(string name, string password)
        {
            if (!await dbAccount.SearchUserWithNameAsync(name))
                throw new Exception("Користувача із цим ім'ям не знайдено");
            Account account = await dbAccount.ReadAccountWithNameAsync(name);

            DeleteAccountDto dto = new DeleteAccountDto()
            {
                AccountId = account.Id,
                Name = account.Name,

                IsDeleted = true
            };

            if (password == account.Password)
            {
                await dbAccount.DeleteAccountAsync(account);
                return dto;
            }
            dto.IsDeleted = false;
            return dto;
        }

        private async Task<string> GenerateIdAsync(int length)
        {
            string id = "";
            do
            {
                id = "";
                for (int i = 0; i < length; i++)
                {
                    id += random.Next(0, 10);
                }
            } while (await dbAccount.SearchUserWithIdAsync(id));

            return id;
        }
        private string GenerateToken(string id, string name)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(Constants.JwtKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity
                (new[]{ 
                        new Claim(ClaimTypes.Name, name), 
                        new Claim(ClaimTypes.NameIdentifier, id) 
                    }),
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
