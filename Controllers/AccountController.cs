using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Monopoly.Models;
using Monopoly.Database;
using Monopoly.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Monopoly.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> logger;
        public AccountController(ILogger<AccountController> logger)
        {
            this.logger = logger;
        }
        [Authorize]
        [HttpGet]
        [ActionName("me")]
        public IActionResult GetMyData()
        {
            string name = User.Identity.Name;
            return Ok(name);
        }
        [HttpPost]
        [ActionName("register")]
        public async Task<IActionResult> Register(Account accModel)
        {
            DBAccount acc = new DBAccount();
            
            await acc.InsertAccountAsync(accModel);
            return Ok("Користувача зареєстровано");
        }
        [HttpPost]
        [ActionName("login")]
        public async Task<IActionResult> Login(Account accModel)
        {
            DBAccount acc = new DBAccount();
            Account account = await acc.ReadAccountAsync(accModel.Name);

            if (accModel.Password == account.Password) 
            {
                string token = GenerateToken(accModel.Name);
                return Ok(token); 
            }
            return Unauthorized("Введено некоректні дані");
        }
        
        [NonAction]
        private string GenerateToken(string name)
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
    }
}
