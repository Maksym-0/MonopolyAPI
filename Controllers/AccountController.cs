using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Monopoly.Core.Interfaces.IServices;
using Monopoly.Core.Models.Request;
using Monopoly.API;
using Monopoly.Core.DTO.Accounts;

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
        public async Task<IActionResult> GetMyData()
        {
            try
            {
                var result = await _accountService.MeAsync(Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
                ApiResponse<AccountDto> response = new ApiResponse<AccountDto>(result.Success, result.Message, result.Data);
                return StatusCode((int)result.StatusCode, response);
            }
            catch(Exception ex)
            {
                return CatchBadRequest(ex);
            }
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AccountRequest dto)
        {
            try
            {
                var result = await _accountService.RegisterAsync(dto.Name, dto.Password);
                ApiResponse<AccountDto> response = new ApiResponse<AccountDto>(result.Success, result.Message, result.Data);
                return StatusCode((int)result.StatusCode, response);
            }
            catch (Exception ex)
            {
                return CatchBadRequest(ex);
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AccountRequest dto)
        {
            try 
            {
                var result = await _accountService.LoginAsync(dto.Name, dto.Password);
                ApiResponse<LoginDto> response = new ApiResponse<LoginDto>(result.Success, result.Message, result.Data);
                return StatusCode((int)result.StatusCode, response);
            }
            catch (Exception ex)
            {
                return CatchBadRequest(ex);
            }
        }
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromBody] AccountRequest dto)
        {
            try
            {
                var result = await _accountService.DeleteAsync(dto.Name, dto.Password);
                ApiResponse<DeleteAccountDto> response = new ApiResponse<DeleteAccountDto>(result.Success, result.Message, result.Data);
                return StatusCode((int)result.StatusCode, response);
            }
            catch (Exception ex)
            {
                return CatchBadRequest(ex);
            }
        }

        private IActionResult CatchBadRequest(Exception ex)
        {
            ApiResponse<object> response = new ApiResponse<object>()
            {
                Success = false,
                Message = ex.Message,
                Data = null
            };
            return BadRequest(response);
        }
    }
}