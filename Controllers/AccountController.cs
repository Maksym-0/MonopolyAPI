using Microsoft.AspNetCore.Mvc;
using Monopoly.Models.ApiResponse;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Monopoly.Interfaces.IServices;
using Newtonsoft.Json.Linq;

namespace Monopoly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetMyData()
        {
            return Ok(new ApiResponse<object>()
            {
                Success = true,
                Message = "Дані користувача",
                Data = new
                {
                    name = User.Identity.Name,
                    id = User.FindFirst(ClaimTypes.NameIdentifier).Value
                }
            });
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(string name, string password)
        {
            if (await _accountService.TryRegisterAsync(name, password))
                return Ok(new ApiResponse<object>()
                {
                    Success = true,
                    Message = "Обліковий запис успішно створено",
                    Data = null
                });
            return BadRequest(new ApiResponse<object>()
            {
                Success = false,
                Message = "Помилка при реєстрації",
                Data = null
            });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(string name, string password)
        {
            string? token = await _accountService.TryLoginAsync(name, password);
            if (token != null)
                return Ok(new ApiResponse<string>()
                {
                    Success = true,
                    Message = "Успішний вхід до облікового запису. Отримано JWT токен",
                    Data = token
                });
            return BadRequest(new ApiResponse<object>()
            {
                Success = false,
                Message = "Введено некоректне ім'я або пароль",
                Data = null
            });
        }
    }
}
