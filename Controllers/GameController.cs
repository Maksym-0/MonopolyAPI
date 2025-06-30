using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monopoly.Database;
using Monopoly.Models;
using Monopoly.Service;

namespace Monopoly.Controllers
{
    [ApiController]
    [Route("api/[controller]/{gameId}")]
    [Authorize]
    public class GameController : ControllerBase
    {
        private readonly ILogger<GameController> logger;
        private GameService gameService = new GameService();
        public GameController(ILogger<GameController> logger)
        {
            this.logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GameStatus(string gameId)
        {
            GameStatus game = await gameService.StatusOfGameAsync(gameId);
            return Ok(game);
        }
        [HttpPut("move")]
        public async Task<IActionResult> Move(string gameId)
        {
            string? check = await gameService.ValidateMoveAsync(gameId, User.Identity.Name);
            if (check != null)
                return BadRequest(check);

            string moveResult = await gameService.MoveAsync(gameId, User.Identity.Name);
            return Ok(moveResult);
        }
        [HttpPut("pay")]
        public async Task<IActionResult> Pay(string gameId)
        {
            string? check = await gameService.ValidatePayAsync(gameId, User.Identity.Name);
            if (check != null)
                return BadRequest(check);

            string? payment = await gameService.TryPayAsync(gameId, User.Identity.Name);
            if (payment != null)
                return Ok(payment);
            return BadRequest("Недостатньо коштів для сплати рахунків");
        }
        [HttpPut("cells/buy")]
        public async Task<IActionResult> BuyCell(string gameId)
        {
            string? check = await gameService.ValidateBuyAsync(gameId, User.Identity.Name);
            if (check != null)
                return BadRequest(check);

            string? buyResult = await gameService.TryBuyAsync(gameId, User.Identity.Name);
            if (buyResult != null)
                return Ok(buyResult);
            return BadRequest();
        }
        [HttpPut("cells/{cellNumber}/levelup")]
        public async Task<IActionResult> LevelUpCell(string gameId, int cellNumber)
        {
            string? check = await gameService.ValidateLevelUpAsync(gameId, User.Identity.Name, cellNumber);
            if (check != null)
                return BadRequest(check);
            else
                return Ok(await gameService.LevelUpAsync(gameId, User.Identity.Name, cellNumber));
        }
        [HttpPut("cells/{cellNumber}/leveldown")]
        public async Task<IActionResult> LevelDownCell(string gameId, int cellNumber)
        {
            DBCells dbCells = new DBCells();
            DBPlayerStatus dbPlayer = new DBPlayerStatus();

            Player player = await dbPlayer.ReadPlayerAsync(gameId, User.Identity.Name);
            Cell cell = await dbCells.ReadCellAsync(gameId, cellNumber);

            string? check = await gameService.ValidateLevelDownAsync(gameId, User.Identity.Name, cellNumber);
            if (check != null)
                return BadRequest(check);
            else
                return Ok(await gameService.LevelDownAsync(gameId, User.Identity.Name, cellNumber));
        }
        [HttpPut("endaction")]
        public async Task<IActionResult> EndAction(string gameId)
        {
            string? check = await gameService.ValidateEndActionAsync(gameId, User.Identity.Name);
            if (check != null)
                return BadRequest(check);
            return Ok(await gameService.EndActionAsync(gameId, User.Identity.Name));
        }
        [HttpPut("leave")]
        public async Task<IActionResult> LeaveGame(string gameId)
        {
            string? check = await gameService.ValidateLeaveAsync(gameId, User.Identity.Name);
            if (check != null)
                return BadRequest(check);

            return Ok(await gameService.LeaveGameAsync(gameId, User.Identity.Name));
        }
    }
}
