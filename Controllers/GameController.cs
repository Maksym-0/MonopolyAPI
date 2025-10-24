using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monopoly.Core.Interfaces.IServices;
using Monopoly.API;
using Monopoly.Core.DTO.Games;

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
                var result = await _gameService.StatsOfGameAsync(Guid.Parse(gameId));
                ApiResponse<GameStateDto> response = new ApiResponse<GameStateDto>(result.Success, result.Message, result.Data);
                return StatusCode((int)result.StatusCode, response);
            }
            catch(Exception ex)
            {
                return CatchBadRequest(ex);
            }
        }
        [HttpPut("move")]
        public async Task<IActionResult> MovePlayer(string gameId)
        {
            try
            {
                var result = await _gameService.MoveAsync(Guid.Parse(gameId), Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
                ApiResponse<MoveDto> response = new ApiResponse<MoveDto>(result.Success, result.Message, result.Data);
                return StatusCode((int)result.StatusCode, response);
            }
            catch(Exception ex)
            {
                return CatchBadRequest(ex);
            }
        }
        [HttpPut("pay/rent")]
        public async Task<IActionResult> PayRent(string gameId)
        {
            try
            {
                var result = await _gameService.PayRentAsync(Guid.Parse(gameId), Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
                ApiResponse<PayDto> response = new ApiResponse<PayDto>(result.Success, result.Message, result.Data);
                return StatusCode((int)result.StatusCode, response); 
            }
            catch (Exception ex)
            {
                return CatchBadRequest(ex);
            }
        }
        [HttpPut("pay/prison")]
        public async Task<IActionResult> PayToLeavePrison(string gameId)
        {
            try
            {
                var result = await _gameService.PayToLeavePrisonAsync(Guid.Parse(gameId), Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
                ApiResponse<PayDto> response = new ApiResponse<PayDto>(result.Success, result.Message, result.Data);
                return StatusCode((int)result.StatusCode, response);
            }
            catch(Exception ex)
            {
                return CatchBadRequest(ex);
            }
        }
        [HttpPut("cells/buy")]
        public async Task<IActionResult> BuyCell(string gameId)
        {
            try
            {
                var result = await _gameService.BuyCellAsync(Guid.Parse(gameId), Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
                ApiResponse<BuyDto> response = new ApiResponse<BuyDto>(result.Success, result.Message, result.Data);
                return StatusCode((int)result.StatusCode, response);
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
                var result = await _gameService.LevelUpAsync(Guid.Parse(gameId), Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value), cellNumber);
                ApiResponse<LevelChangeDto> response = new ApiResponse<LevelChangeDto>(result.Success, result.Message, result.Data);
                return StatusCode((int)result.StatusCode, response);
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
                var result = await _gameService.LevelDownAsync(Guid.Parse(gameId), Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value), cellNumber);
                ApiResponse<LevelChangeDto> response = new ApiResponse<LevelChangeDto>(result.Success, result.Message, result.Data);
                return StatusCode((int)result.StatusCode, response);
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
                var result = await _gameService.EndActionAsync(Guid.Parse(gameId), Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
                ApiResponse<NextActionDto> response = new ApiResponse<NextActionDto>(result.Success, result.Message, result.Data);
                return StatusCode((int)result.StatusCode, response); 
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
                var result = await _gameService.LeaveGameAsync(Guid.Parse(gameId), Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
                ApiResponse<LeaveGameDto> response = new ApiResponse<LeaveGameDto>(result.Success, result.Message, result.Data);
                return StatusCode((int)result.StatusCode, response);
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
