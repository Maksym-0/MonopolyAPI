using Microsoft.AspNetCore.Mvc;
using Monopoly.Models.ApiResponse;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Monopoly.Interfaces.IServices;
using Monopoly.Models.Request;

namespace Monopoly.Controllers
{
    [ApiController]
    [Route("api/account")]
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
                    Id = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                    Name = User.Identity.Name
                }
            });
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AccountRequest dto)
        {
            try
            {
                await _accountService.TryRegisterAsync(dto.Name, dto.Password);
                return Ok(new ApiResponse<object>()
                {
                    Success = true,
                    Message = "Обліковий запис успішно створено",
                    Data = null
                });
            }
            catch(Exception ex)
            {
                return BadRequest(new ApiResponse<object>()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AccountRequest dto)
        {
            string? token;
            try { token = await _accountService.TryLoginAsync(dto.Name, dto.Password); }
            catch(Exception ex) {
                return BadRequest(new ApiResponse<string>()
                {
                    Success = false,
                    Message = "Введено некоректне ім'я або пароль",
                    Data = ex.Message
                });
            }
            if (token != null)
                return Ok(new ApiResponse<string>()
                {
                    Success = true,
                    Message = "Успішний вхід до облікового запису. Отримано JWT токен",
                    Data = token
                });
            return BadRequest(new ApiResponse<string>()
            {
                Success = false,
                Message = "Введено некоректне ім'я або пароль",
                Data = null
            });
        }
    }
}
