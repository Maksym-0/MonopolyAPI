using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Monopoly.Database;
using Monopoly.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Monopoly.Service
{
    public class AccountService
    {
        public async Task<string?> TryRegisterAsync(Account accModel)
        {
            if (await CheckName(accModel.Name))
                return null;

            DBAccount acc = new DBAccount();

            await acc.InsertAccountAsync(accModel);
            return "Користувача зареєстровано";
        }
        public async Task<string?> TryLoginAsync(string name, string password)
        {
            DBAccount acc = new DBAccount();
            Account account = await acc.ReadAccountAsync(name);

            if (password == account.Password)
            {
                string token = GenerateToken(account.ID, account.Name);
                return token;
            }
            return null;
        }
        
        private string GenerateToken(string id, string name)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(Constants.JwtKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, name) }),
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private async Task<bool> CheckName(string name)
        {
            DBAccount dbAccount = new DBAccount();
            List<Account> players = await dbAccount.ReadAccountListAsync();
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Name == name)
                    return true;
            }
            return false;
        }
    }
}
