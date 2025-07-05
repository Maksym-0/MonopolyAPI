using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monopoly.Interfaces.IServices;
using Monopoly.Models.ApiResponse;
using Monopoly.Models.APIResponse;
using System.Security.Claims;

namespace Monopoly.Controllers
{
    [ApiController]
    [Route("api/[controller]/{gameId}")]
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
                GameStatus game = await _gameService.StatusOfGameAsync(gameId);
                ApiResponse<GameStatus> response = new ApiResponse<GameStatus>()
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
                string moveResult = await _gameService.MoveAsync(gameId, User.FindFirst(ClaimTypes.NameIdentifier).Value);
                ApiResponse<string> response = new ApiResponse<string>()
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
                bool payResult = await _gameService.TryPayAsync(gameId, User.FindFirst(ClaimTypes.NameIdentifier).Value);
                ApiResponse<bool> response = new ApiResponse<bool>()
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
                bool buyResult = await _gameService.TryBuyAsync(gameId, User.FindFirst(ClaimTypes.NameIdentifier).Value);
                ApiResponse<bool> response = new ApiResponse<bool>()
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
                await _gameService.LevelUpAsync(gameId, User.FindFirst(ClaimTypes.NameIdentifier).Value, cellNumber);
                ApiResponse<string> response = new ApiResponse<string>()
                {
                    Success = true,
                    Message = "Збільшення рівня клітини завершено",
                    Data = null
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
                await _gameService.LevelDownAsync(gameId, User.FindFirst(ClaimTypes.NameIdentifier).Value, cellNumber);
                ApiResponse<string> response = new ApiResponse<string>()
                {
                    Success = true,
                    Message = "Зменшення рівня клітини завершено",
                    Data = null
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
                string status = await _gameService.EndActionAsync(gameId, User.FindFirst(ClaimTypes.NameIdentifier).Value);
                ApiResponse<string> response = new ApiResponse<string>()
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
                string status = await _gameService.LeaveGameAsync(gameId, User.FindFirst(ClaimTypes.NameIdentifier).Value);
                ApiResponse<string> response = new ApiResponse<string>()
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
            ApiResponse<List<RoomResponse>> response = new ApiResponse<List<RoomResponse>>()
            {
                Success = false,
                Message = ex.Message,
                Data = null
            };
            return BadRequest(response);
        }
    }
}
