using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monopoly.Interfaces.IServices;
using Monopoly.Models.ApiResponse;
using System.Security.Claims;

namespace Monopoly.Controllers
{
    [ApiController]
    [Route("api/game/{gameId}")]
    [Authorize]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet]
        public async Task<IActionResult> GameStatus(string gameId)
        {
            try
            {
                GameDto game = await _gameService.StatusOfGameAsync(gameId);
                ApiResponse<GameDto> response = new ApiResponse<GameDto>()
                {
                    Success = true,
                    Message = "Отримано поточний стан гри",
                    Data = game
                };
                return Ok(response);
            }
            catch(Exception ex)
            {
                return CatchBadRequest(ex);
            }
        }
        [HttpPut("move")]
        public async Task<IActionResult> Move(string gameId)
        {
            try
            {
                var moveResult = await _gameService.MoveAsync(gameId, User.FindFirst(ClaimTypes.NameIdentifier).Value);
                ApiResponse<MoveDto> response = new ApiResponse<MoveDto>()
                {
                    Success = true,
                    Message = "Бросок кубиків та відповідний рух завершено",
                    Data = moveResult
                };
                return Ok(response);
            }
            catch(Exception ex)
            {
                return CatchBadRequest(ex);
            }
        }
        [HttpPut("pay")]
        public async Task<IActionResult> Pay(string gameId)
        {
            try
            {
                var payResult = await _gameService.TryPayAsync(gameId, User.FindFirst(ClaimTypes.NameIdentifier).Value);
                ApiResponse<PayDto> response = new ApiResponse<PayDto>()
                {
                    Success = true,
                    Message = "Сплату рахунків завершено",
                    Data = payResult
                };
                return Ok(response); 
            }
            catch (Exception ex)
            {
                return CatchBadRequest(ex);
            }
        }
        [HttpPut("cells/buy")]
        public async Task<IActionResult> BuyCell(string gameId)
        {
            try
            {
                var buyResult = await _gameService.TryBuyAsync(gameId, User.FindFirst(ClaimTypes.NameIdentifier).Value);
                ApiResponse<BuyDto> response = new ApiResponse<BuyDto>()
                {
                    Success = true,
                    Message = "Придбання клітини завершено",
                    Data = buyResult
                };
                return Ok(response);
            }
            catch(Exception ex)
            {
                return CatchBadRequest(ex);
            }
        }
        [HttpPut("cells/{cellNumber}/levelup")]
        public async Task<IActionResult> LevelUpCell(string gameId, int cellNumber)
        {
            try
            {
                var levelUpResult = await _gameService.LevelUpAsync(gameId, User.FindFirst(ClaimTypes.NameIdentifier).Value, cellNumber);
                ApiResponse<LevelChangeDto> response = new ApiResponse<LevelChangeDto>()
                {
                    Success = true,
                    Message = "Збільшення рівня клітини завершено",
                    Data = levelUpResult
                };
                return Ok(response);
            }
            catch(Exception ex)
            {
                return CatchBadRequest(ex);
            }
        }
        [HttpPut("cells/{cellNumber}/leveldown")]
        public async Task<IActionResult> LevelDownCell(string gameId, int cellNumber)
        {
            try
            {
                var levelDownResult = await _gameService.LevelDownAsync(gameId, User.FindFirst(ClaimTypes.NameIdentifier).Value, cellNumber);
                ApiResponse<LevelChangeDto> response = new ApiResponse<LevelChangeDto>()
                {
                    Success = true,
                    Message = "Зменшення рівня клітини завершено",
                    Data = levelDownResult
                };
                return Ok(response);
            }
            catch(Exception ex)
            {
                return CatchBadRequest(ex);
            }
        }
        [HttpPut("endaction")]
        public async Task<IActionResult> EndAction(string gameId)
        {
            try
            {
                var status = await _gameService.EndActionAsync(gameId, User.FindFirst(ClaimTypes.NameIdentifier).Value);
                ApiResponse<NextActionDto> response = new ApiResponse<NextActionDto>()
                {
                    Success = true,
                    Message = "Дію завершено",
                    Data = status
                };
                return Ok(response); 
            }
            catch(Exception ex)
            {
                return CatchBadRequest(ex);
            }
        }
        [HttpPut("leave")]
        public async Task<IActionResult> LeaveGame(string gameId)
        {
            try
            {
                var status = await _gameService.LeaveGameAsync(gameId, User.FindFirst(ClaimTypes.NameIdentifier).Value);
                ApiResponse<LeaveGameDto> response = new ApiResponse<LeaveGameDto>()
                {
                    Success = true,
                    Message = "Гру покинуто",
                    Data = status
                };
                return Ok(response);
            }
            catch(Exception ex)
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
