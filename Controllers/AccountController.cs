using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Monopoly.Models;
using Microsoft.AspNetCore.Authorization;
using Monopoly.Service;

namespace Monopoly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> logger;
        private AccountService accountService = new AccountService();
        public AccountController(ILogger<AccountController> logger)
        {
            this.logger = logger;
        }
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetMyData()
        {
            return Ok(User.Identity.Name);
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(Account accModel)
        {
            string? status = await accountService.TryRegisterAsync(accModel);
            if (status != null)
                return Ok(status);
            return BadRequest("Користувача з обраним ім'м вже зареєстровано");
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(string name, string password)
        {
            string? status = await accountService.TryLoginAsync(name, password);
            if (status != null)
                return Ok(status);
            return BadRequest("Введено некоректне ім'я або пароль");
        }
    }
}
