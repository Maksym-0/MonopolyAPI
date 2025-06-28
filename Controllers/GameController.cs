using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monopoly.Database;
using Monopoly.Models;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata.Ecma335;

namespace Monopoly.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Authorize]
    public class GameController : ControllerBase
    {
        private readonly ILogger<GameController> logger;
        public GameController(ILogger<GameController> logger)
        {
            this.logger = logger;
        }
        [HttpGet]
        [ActionName("GameStatus")]
        public async Task<IActionResult> GetGameStatus(string gameId)
        {
            DBCells dbCells = new DBCells();
            DBPlayerStatus dbPlayer = new DBPlayerStatus();

            List<Player> players = new List<Player>();
            List<Cell> cells = new List<Cell>();

            cells = await dbCells.ReadCellListAsync(gameId);
            players = await dbPlayer.ReadPlayerListAsync(gameId);

            GameStatus game = new GameStatus(gameId, cells, players);
            return Ok(game);
        }
        [HttpPost]
        [ActionName("Move")]
        public async Task<IActionResult> Move(string gameId)
        {
            DBCells dbCells = new DBCells();
            DBPlayerStatus dbPlayer = new DBPlayerStatus();

            Player player = await dbPlayer.ReadPlayerAsync(gameId, User.Identity.Name);

            if (!player.HisAction)
                return BadRequest("Гравець не може ходити не в свій хід");
            else if (!player.CanMove)
                return BadRequest("Гравець більше не може кидати кубики та рухатись");

            List<Player> players = new List<Player>();
            List<Cell> cells = new List<Cell>();
            cells = await dbCells.ReadCellListAsync(gameId);
            players = await dbPlayer.ReadPlayerListAsync(gameId);

            Dice dice = new Dice();

            player.CanMove = false;

            int oldLocation = player.Location;
            if(player.ReverseMove > 0)
            {
                player.Location = (player.Location - dice.DiceSum + cells.Count) % cells.Count;
                player.ReverseMove -= 1;
            }
            else
            { 
                player.Location = (player.Location + dice.DiceSum) % cells.Count;
                if (player.Location < oldLocation)
                    player.Balance += 100;
            }

            Cell cell = cells[player.Location];
            if (Constants.SpecialCellNames.Contains(cell.Name))
            {
                Random random = new Random();
                switch (cell.Name)
                {
                    case "Старт":
                        player.Balance += 300;
                        await dbPlayer.UpdatePlayerAsync(player);
                        return Ok("Ви потрапили на клітину Старт. Отримали гроші");
                    case "В'язниця":
                        player.Location = 30;
                        await dbPlayer.UpdatePlayerAsync(player);
                        return Ok("Ви потрапили на клітину В'язниця. Їдьте на відпочинок");
                    case "Казино":
                        int win = random.Next(-3, 4) * 100;
                        player.Balance += win;
                        await dbPlayer.UpdatePlayerAsync(player);
                        return Ok($"Ви потрапили на клітину Казино. Ваш виграш: {win}");
                    case "Відпочинок":
                        player.Location = 11;
                        player.CantAction = 3;
                        player.IsPrisoner = true;
                        await dbPlayer.UpdatePlayerAsync(player);
                        return Ok("Ви потрапили на клітину Відпочинок. Ви потрапили до тюрми");
                    case "Мінус гроші":
                        int lose = random.Next(-3, 0) * 100;
                        player.Balance += lose;
                        await dbPlayer.UpdatePlayerAsync(player);
                        return Ok($"Ви потрапили на клітину Мінус гроші. Ви втратили {lose}");
                    case "Плюс гроші":
                        int plus = random.Next(1, 4) * 100;
                        player.Balance += plus;
                        await dbPlayer.UpdatePlayerAsync(player);
                        return Ok($"Ви потрапили на клітину Плюс гроші. Ви отримали {plus}");
                    case "Швидка допомога":
                        player.CantAction = 1;
                        await dbPlayer.UpdatePlayerAsync(player);
                        return Ok("Ви потрапили на клітину Швидка допомога. Пропустіть один хід");
                    case "Зворотній хід":
                        player.ReverseMove = 1;
                        await dbPlayer.UpdatePlayerAsync(player);
                        return Ok("Ви потрапили на клітину Зворотній хід. Наступний хід відбудеться у зворотньому напрямку");
                    case "Торнадо":
                        return NotFound("Можливість не реалізовано");
                    default:
                        return NotFound("Невідома особлива клітина");
                }
            }
            else if (cell.OwnerName == null)
            {
                player.CanBuyCell = true;
                await dbPlayer.UpdatePlayerAsync(player);
                return Ok("Рух завершено. Клітину можна купити");
            }
            else
            {
                player.NeedPay = true;
                await dbPlayer.UpdatePlayerAsync(player);
                return Ok("Ви потрапили на територію чужої компанії. Необхідно сплатити рахунки");
            }
        }
        [HttpPost]
        [ActionName("Pay")]
        public async Task<IActionResult> Pay(string gameId)
        {
            DBCells dbCells = new DBCells();
            DBPlayerStatus dbPlayer = new DBPlayerStatus();

            Player player = await dbPlayer.ReadPlayerAsync(gameId, User.Identity.Name);

            if (!player.InGame)
                return BadRequest("Гравець поза грою");
            else if (!player.NeedPay)
                return BadRequest("Гравець не повинен платити");

            Cell cell = await dbCells.ReadCellAsync(gameId, player.Location);
            if (Constants.SpecialCellNames.Contains(cell.Name))
            {
                if(player.IsPrisoner == true)
                {
                    if (player.PayToLeavePrison())
                    {
                        await dbPlayer.UpdatePlayerAsync(player);
                        return Ok("Ви покинули в'язницю!");
                    }
                    else return BadRequest("Недостатньо коштів щоб покинути в'язицю");
                }
            }
            if (player.Balance < cell.Rent) 
            { 
                return Conflict("Гравцеві недостатньо коштів для оплати"); 
            }
            else 
            {
                player.PayRent(Convert.ToInt32(cell.Rent));
                await dbPlayer.UpdatePlayerAsync(player);
                return Ok("Оплату проведено");
            }
        }
        [HttpPost]
        [ActionName("BuyCell")]
        public async Task<IActionResult> BuyCell(string gameId)
        {
            DBCells dbCells = new DBCells();
            DBPlayerStatus dbPlayer = new DBPlayerStatus();

            Player player = await dbPlayer.ReadPlayerAsync(gameId, User.Identity.Name);

            if (!player.InGame)
                return BadRequest("Гравець поза грою");
            else if (!player.CanBuyCell)
                return BadRequest("Гравець не може придбати клітину");

            Cell cell = await dbCells.ReadCellAsync(gameId, player.Location);
            if (cell.OwnerName != null)
                return Conflict("Неможливо придбати клітину, що належить іншому гравцю");
            if (player.Balance >= cell.Price)
            {
                player.Buy(Convert.ToInt32(cell.Price));
                cell.OwnerName = player.Name;
                await dbPlayer.UpdatePlayerAsync(player);
                await dbCells.UpdateCellAsync(cell);
                return Ok("Клітину придбано");
            }
            else return BadRequest("Недостатньо коштів на рахунку");
        }
        [HttpPost]
        [ActionName("LevelUpCell")]
        public async Task<IActionResult> LevelUpCell(string gameId, int cellNumber)
        {
            DBCells dbCells = new DBCells();
            DBPlayerStatus dbPlayer = new DBPlayerStatus();

            Player player = await dbPlayer.ReadPlayerAsync(gameId, User.Identity.Name);

            if (!player.InGame)
                return BadRequest("Гравець поза грою");
            else if (!player.CanUpdateCell)
                return BadRequest("Гравець не може підняти рівень клітини");

            Cell cell = await dbCells.ReadCellAsync(gameId, cellNumber);
            if (Constants.SpecialCellNames.Contains(Constants.CellNames[cellNumber]))
                return BadRequest("Неможливо змінити рівень особливої клітини");
            else if (!await cell.CheckMonopoly())
                return BadRequest("Відсутня монополія, підняти рівень клітини неможливо");
            else if (cell.OwnerName != User.Identity.Name)
                return BadRequest("Заборонено змінювти рівень клітини, що Вам не належить");
            else if (cell.Level == 5)
                return BadRequest("Рівень обраної клітини вже є максимальний");
            else if (player.Balance < Constants.CellBuildAndSellCost[cellNumber])
                return BadRequest("Вам не стане коштів на це будівництво");
            else
            {
                player.PayToUpgrade(Constants.CellBuildAndSellCost[cellNumber]);
                cell.ChangeCellLevel(cell.Level + 1);
                await dbPlayer.UpdatePlayerAsync(player);
                await dbCells.UpdateCellAsync(cell);
                return Ok("Рівень клітини піднято");
            }
        }
        [HttpPost]
        [ActionName("LevelDownCell")]
        public async Task<IActionResult> LevelDownCell(string gameId, int cellNumber)
        {
            DBCells dbCells = new DBCells();
            DBPlayerStatus dbPlayer = new DBPlayerStatus();

            Player player = await dbPlayer.ReadPlayerAsync(gameId, User.Identity.Name);

            if (!player.InGame)
                return BadRequest("Гравець поза грою");

            Cell cell = await dbCells.ReadCellAsync(gameId, cellNumber);
            if (cell.OwnerName != User.Identity.Name)
                return BadRequest("Заборонено змінювти рівень клітини, що Вам не належить");
            else if (cell.Level == 0)
                return BadRequest("Рівень обраної клітини вже є мінімальний");
            else
            {
                player.Balance += Constants.CellBuildAndSellCost[cellNumber];
                cell.ChangeCellLevel(cell.Level - 1);
                await dbPlayer.UpdatePlayerAsync(player);
                await dbCells.UpdateCellAsync(cell);
                return Ok("Рівень клітини знижено");
            }
        }
        [HttpPost]
        [ActionName("EndAction")]
        public async Task<IActionResult> EndAction(string gameId)
        {
            DBCells dbCells = new DBCells();
            DBPlayerStatus dbPlayer = new DBPlayerStatus();

            Player player = await dbPlayer.ReadPlayerAsync(gameId, User.Identity.Name);

            if (!player.InGame)
                return BadRequest("Гравець вже поза грою");
            else if (!player.HisAction)
                return BadRequest("Гравець не може завершити нерозпочатий хід");
            else if (player.CanMove)
                return BadRequest("Гравець не може завершити хід, не кинувши кубики");
            else if (player.NeedPay)
                return BadRequest("Гравець не може завершити хід, не оплативши рахунки");

            player.StopAction();
            await dbPlayer.UpdatePlayerAsync(player);
            string newPlayer = await ActionNextPlayer(gameId, User.Identity.Name);
            return Ok($"Хід гравця завершено та передано {newPlayer}");
        }
        [HttpPost]
        [ActionName("LeaveGame")]
        public async Task<IActionResult> LeaveGame(string gameId)
        {
            DBCells dbCells = new DBCells();
            DBPlayerStatus dbPlayer = new DBPlayerStatus();

            Player player = await dbPlayer.ReadPlayerAsync(gameId, User.Identity.Name);

            if (!player.InGame)
                return BadRequest("Гравець вже поза грою");

            player.StopAction();
            player.InGame = false;
            
            List<Cell> cells = await dbCells.ReadCellListAsync(gameId);
            
            for(int i = 0; i < cells.Count; i++)
            {
                Cell cell = cells[i];
                if(cell.OwnerName == User.Identity.Name)
                {
                    cell.Price = Constants.CellPrices[i];
                    cell.Rent = Constants.CellStartRents[i];
                    cell.OwnerName = null;
                    cell.Level = 0;
                    await dbCells.UpdateCellAsync(cell);
                }
            }
            string? winner = await ChekWin(gameId);
            if(winner != null) 
            {
                DeleteGame(gameId);
                return Ok($"Гравець покинув гру. Переможець: {winner}"); 
            }
            return Ok("Гравець покинув гру");
        }

        [NonAction]
        public async Task<string?> ChekWin(string gameId)
        {
            DBPlayerStatus playerStatus = new DBPlayerStatus();

            List<Player> players = await playerStatus.ReadPlayerListAsync(gameId);
            int playersInGame = 0;
            string winner = "";
            for (int i = 0; i < players.Count; i++) 
            {
                if (players[i].InGame) 
                {
                    playersInGame++;
                    winner = players[i].Name;
                } 
            }
            if (playersInGame == 1) return winner;
            return null;
        }
        [NonAction]
        public async Task DeleteGame(string gameId)
        {
            DBPlayerStatus dbPlayer = new DBPlayerStatus();
            DBCells dbCells = new DBCells();
            DBRoom dbRoom = new DBRoom();
            dbRoom.DeleteRoomAsync(gameId);
            await Task.Delay(TimeSpan.FromMinutes(3));

            dbCells.DeleteCellsAsync(gameId);
            dbPlayer.DeletePlayersAsync(gameId);
        }
        [NonAction]
        public async Task<string> ActionNextPlayer(string gameId, string oldPlayerName)
        {
            DBPlayerStatus dbPlayer = new DBPlayerStatus();
            List<Player> players = await dbPlayer.ReadPlayerListAsync(gameId);
            Player nextPlayer = players[players.FindIndex(p => p.Name == oldPlayerName)];
            while (true)
            {
                nextPlayer = players[(players.FindIndex(p => p.Name == nextPlayer.Name) + 1) % players.Count];

                if(nextPlayer.InGame == false) continue;
                else if (nextPlayer.IsPrisoner == true)
                {
                    if (nextPlayer.CantAction < 1) 
                    { 
                        nextPlayer.IsPrisoner = false;
                        nextPlayer.StartAction();
                        await dbPlayer.UpdatePlayerAsync(nextPlayer);
                        return nextPlayer.Name;
                    }
                    else 
                    {
                        nextPlayer.CantAction--;
                        await dbPlayer.UpdatePlayerAsync(nextPlayer);
                    }
                }
                else if (nextPlayer.CantAction > 0)
                {
                    nextPlayer.CantAction--;
                    await dbPlayer.UpdatePlayerAsync(nextPlayer);
                }
                else
                {
                    nextPlayer.StartAction();
                    await dbPlayer.UpdatePlayerAsync(nextPlayer);
                    return nextPlayer.Name;
                }
            }
        }
    }
}
