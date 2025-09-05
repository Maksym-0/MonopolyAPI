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
            AccountDto account = new AccountDto()
            {
                Id = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                Name = User.Identity.Name
            };
            return Ok(new ApiResponse<object>()
            {
                Success = true,
                Message = "Дані користувача",
                Data = account
            });
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AccountRequest dto)
        {
            try
            {
                AccountDto accountDto = await _accountService.TryRegisterAsync(dto.Name, dto.Password);
                return Ok(new ApiResponse<object>()
                {
                    Success = true,
                    Message = "Обліковий запис успішно створено",
                    Data = accountDto
                });
            }
            catch (Exception ex)
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
            LoginDto? loginDto;
            try 
            {
                loginDto = await _accountService.TryLoginAsync(dto.Name, dto.Password); 
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string>()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            if (loginDto != null)
                return Ok(new ApiResponse<LoginDto>()
                {
                    Success = true,
                    Message = "Успішний вхід до облікового запису. Отримано JWT токен",
                    Data = loginDto
                });
            return BadRequest(new ApiResponse<string>()
            {
                Success = false,
                Message = "Введено некоректне ім'я або пароль",
                Data = null
            });
        }
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromBody] AccountRequest dto)
        {
            try
            {
                DeleteAccountDto result = await _accountService.TryDeleteAsync(dto.Name, dto.Password);
                if (result.IsDeleted)
                {
                    return Ok(new ApiResponse<DeleteAccountDto>()
                    { 
                        Success = true,
                        Message = "Обліковий запис успішно видалено",
                        Data = result
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<DeleteAccountDto>()
                    {
                        Success = false,
                        Message = "Невірний пароль",
                        Data = result
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
        }
    }
}