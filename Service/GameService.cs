using Monopoly.Interfaces.IServices;
using Monopoly.Interfaces.IDatabases;
using Monopoly.Models.GameModels;
using Monopoly.Models.APIResponse;
namespace Monopoly.Service
{
    public class GameService : IGameService
    {
        private Random random = new Random();
        private readonly ICellRepository dbCells;
        private readonly IPlayerRepository dbPlayer;
        private readonly IRoomRepository dbRoom;
        private readonly IPlayerInRoomRepository dbPlayerInRoom;

        public GameService(ICellRepository cellRepository, IPlayerRepository playerRepository, 
            IRoomRepository roomRepository, IPlayerInRoomRepository playerInRoomRepository) 
        {
            dbCells = cellRepository;
            dbPlayer = playerRepository;
            dbRoom = roomRepository;
            dbPlayerInRoom = playerInRoomRepository;
        }

        public async Task<GameReponse> StatusOfGameAsync(string gameId)
        {
            List<Player> players = await dbPlayer.ReadPlayerListAsync(gameId);
            List<Cell> cells = await dbCells.ReadCellListAsync(gameId);

            GameReponse gameStatus = new GameReponse(gameId, cells, players);

            return gameStatus;
        }
        public async Task<string> MoveAsync(string gameId, string playerId)
        {
            string? isValid = await ValidateMoveAsync(gameId, playerId);
            if (isValid != null)
                throw new Exception(isValid);
            
            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);
            List<Cell> cells = await dbCells.ReadCellListAsync(gameId);

            Dice dice = new Dice();

            player.CanMove = false;
            player.LastDiceResult = dice;

            int oldLocation = player.Location;
            if (player.ReverseMove > 0)
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
            return await CellEffectAsync(player, cells);
        }
        public async Task<bool> TryPayAsync(string gameId, string playerId)
        {
            string? isValid = await ValidatePayAsync(gameId, playerId);
            if (isValid != null)
                throw new Exception(isValid);

            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);
            Cell cell = await dbCells.ReadCellAsync(gameId, player.Location);

            if (Constants.SpecialCellNames.Contains(cell.Name))
            {
                if (player.IsPrisoner == true)
                {
                    if (player.PayToLeavePrison())
                    {
                        await dbPlayer.UpdatePlayerAsync(player);
                        return true;
                    }
                    return false;
                }
            }
            if (player.Balance < cell.Rent)
            {
                return false;
            }
            else
            {
                player.PayRent(Convert.ToInt32(cell.Rent));
                await dbPlayer.UpdatePlayerAsync(player);
                return true;
            }
        }
        public async Task<bool> TryBuyAsync(string gameId, string playerId)
        {
            string? isValid = await ValidateBuyAsync(gameId, playerId);
            if (isValid != null)
                throw new Exception(isValid);

            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);
            Cell cell = await dbCells.ReadCellAsync(gameId, player.Location);
            
            player.DeductMoney(Convert.ToInt32(cell.Price));
            cell.Owner = player.Name;
            
            await dbPlayer.UpdatePlayerAsync(player);
            await dbCells.UpdateCellAsync(cell);
            return true;
        }
        public async Task LevelUpAsync(string gameId, string playerId, int cellNumber)
        {
            string? isValid = await ValidateLevelUpAsync(gameId, playerId, cellNumber);
            if (isValid != null)
                throw new Exception(isValid);

            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);
            Cell cell = await dbCells.ReadCellAsync(gameId, cellNumber);

            player.PayToUpgrade(Constants.CellBuildAndSellCost[cellNumber]);
            cell.ChangeCellLevel(cell.Level + 1);
            
            await dbPlayer.UpdatePlayerAsync(player);
            await dbCells.UpdateCellAsync(cell);
        }
        public async Task LevelDownAsync(string gameId, string playerId, int cellNumber)
        {
            string? isValid = await ValidateLevelDownAsync(gameId, playerId, cellNumber);
            if (isValid != null)
                throw new Exception(isValid);

            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);
            Cell cell = await dbCells.ReadCellAsync(gameId, cellNumber);

            player.Balance += Constants.CellBuildAndSellCost[cellNumber];
            cell.ChangeCellLevel(cell.Level - 1);
            
            await dbPlayer.UpdatePlayerAsync(player);
            await dbCells.UpdateCellAsync(cell);
        }
        public async Task<string> EndActionAsync(string gameId, string playerId)
        {
            string? isValid = await ValidateEndActionAsync(gameId, playerId);
            if (isValid != null)
                throw new Exception(isValid);

            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);

            player.StopAction();
            await dbPlayer.UpdatePlayerAsync(player);
            
            string newPlayer = await ActionNextPlayer(gameId, playerId);
            return $"Хід гравця завершено та передано {newPlayer}";
        }
        public async Task<string> LeaveGameAsync(string gameId, string playerId)
        {
            string? isValid = await ValidateLeaveAsync(gameId, playerId);
            if (isValid != null)
                throw new Exception(isValid);

            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);
            player.StopAction();
            player.InGame = false;
            await dbPlayer.UpdatePlayerAsync(player);
            await dbPlayerInRoom.DeletePlayerInRoomAsync(playerId);

            var room = await dbRoom.ReadRoomAsync(gameId);
            room.CountOfPlayers -= 1;
            await dbRoom.UpdateRoomAsync(room);

            List<Cell> cells = await dbCells.ReadCellListAsync(gameId);

            for (int i = 0; i < cells.Count; i++)
            {
                Cell cell = cells[i];
                if (cell.Owner == playerId)
                {
                    cell.Price = Constants.CellPrices[i];
                    cell.Rent = Constants.CellStartRents[i];
                    cell.Owner = null;
                    cell.Level = 0;
                    await dbCells.UpdateCellAsync(cell);
                }
            }
            string? winner = await ChekWinner(gameId);
            if (winner != null)
            {
                await DeleteGameAndRoom(gameId);
                return $"Гравець {player.Name} покинув гру. Переможець: {winner}";
            }
            return $"Гравець {player.Name} покинув гру";
        }

        private async Task<string?> ValidateMoveAsync(string gameId, string playerId)
        {
            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);

            if (!player.InGame)
                return "Гравець поза грою";
            else if (!player.HisAction)
                return "Гравець не може ходити не в свій хід";
            else if (!player.CanMove)
                return "Гравець більше не може кидати кубики та рухатись";
            return null;
        }
        private async Task<string?> ValidatePayAsync(string gameId, string playerId)
        {
            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);

            if (!player.InGame)
                return "Гравець поза грою";
            else if (!player.NeedPay)
                return "Гравець не повинен платити";
            return null;
        }
        private async Task<string?> ValidateBuyAsync(string gameId, string playerId)
        {
            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);
            Cell cell = await dbCells.ReadCellAsync(gameId, player.Location);

            if (!player.InGame)
                return "Гравець поза грою";
            else if (cell.Unique)
                return "Неможливо придбати особливу клітину";
            else if (!player.CanBuyCell)
                return "Гравець не може придбати клітину";
            else if (cell.Owner != null)
                return "Неможливо придбати клітину, що належить іншому гравцю";
            else if (player.Balance < cell.Price)
                return "Неможливо придбати клітину. Недостатньо коштів";
            return null;
        }
        private async Task<string?> ValidateLevelUpAsync(string gameId, string playerId, int cellNumber)
        {
            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);
            Cell cell = await dbCells.ReadCellAsync(gameId, cellNumber);

            if (!player.InGame)
                return "Гравець поза грою";
            else if (!player.CanLevelUpCell)
                return "Гравець не може підняти рівень клітини";
            else if (cell.Unique)
                return "Неможливо змінити рівень особливої клітини";
            else if (!await CheckMonopoly(cell))
                return "Відсутня монополія, підняти рівень клітини неможливо";
            else if (cell.Owner != playerId)
                return "Заборонено змінювти рівень клітини, що Вам не належить";
            else if (cell.Level == 5)
                return "Рівень обраної клітини вже є максимальний";
            else if (player.Balance < Constants.CellBuildAndSellCost[cellNumber])
                return "Вам не стане коштів на це будівництво";
            return null;
        }
        private async Task<string?> ValidateLevelDownAsync(string gameId, string playerId, int cellNumber)
        {
            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);
            Cell cell = await dbCells.ReadCellAsync(gameId, cellNumber);

            if (!player.InGame)
                return "Гравець поза грою";
            else if(cell.Unique)
                return "Неможливо знизити рівень особливої клітини";
            else if (!player.HisAction)
                return "Гравець не може змінювати рівень клітин не в свій хід";
            else if (cell.Owner != playerId)
                return "Заборонено змінювти рівень клітини, що Вам не належить";
            else if (cell.Level == 0)
                return "Рівень обраної клітини вже є мінімальний";
            return null;
        }
        private async Task<string?> ValidateEndActionAsync(string gameId, string playerId)
        {
            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);

            if (!player.InGame)
                return "Гравець вже поза грою";
            else if (!player.HisAction)
                return "Гравець не може завершити нерозпочатий хід";
            else if (player.CanMove)
                return "Гравець не може завершити хід, не кинувши кубики";
            else if (player.NeedPay)
                return "Гравець не може завершити хід, не оплативши рахунки";
            return null;
        }
        private async Task<string?> ValidateLeaveAsync(string gameId, string playerId)
        {
            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);

            if (!player.InGame)
                return "Гравець вже поза грою";
            return null;
        }

        private async Task<string> CellEffectAsync(Player player, List<Cell> cells)
        {
            Cell cell = cells[player.Location];
            string message;

            if (cell.Unique)
            {
                switch (cell.Name)
                {
                    case "Старт":
                        player.Balance += 300;
                        message = "Ви потрапили на клітину Старт. Отримали гроші";
                        break;
                    case "В'язниця":
                        player.Location = 30;
                        message = "Ви потрапили на клітину В'язниця. Їдьте на відпочинок";
                        break;
                    case "Казино":
                        int win = random.Next(-3, 4) * 100;
                        player.Balance += win;
                        message = $"Ви потрапили на клітину Казино. Ваш виграш: {win}";
                        break;
                    case "Відпочинок":
                        player.Location = 11;
                        player.CantAction = 3;
                        player.IsPrisoner = true;
                        message = "Ви потрапили на клітину Відпочинок. Ви потрапили до тюрми";
                        break;
                    case "Мінус гроші":
                        int lose = random.Next(-3, 0) * 100;
                        player.Balance += lose;
                        message = $"Ви потрапили на клітину Мінус гроші. Ви втратили {lose}";
                        break;
                    case "Плюс гроші":
                        int plus = random.Next(1, 4) * 100;
                        player.Balance += plus;
                        message = $"Ви потрапили на клітину Плюс гроші. Ви отримали {plus}";
                        break;
                    case "Швидка допомога":
                        player.CantAction = 1;
                        message = "Ви потрапили на клітину Швидка допомога. Пропустіть один хід";
                        break;
                    case "Зворотній хід":
                        player.ReverseMove = 1;
                        message = "Ви потрапили на клітину Зворотній хід. Наступний хід відбудеться у зворотньому напрямку";
                        break;
                    default:
                        message = "Невідома особлива клітина";
                        break;
                }
            }
            else if (cell.Owner == null)
            {
                player.CanBuyCell = true;
                message = "Рух завершено. Клітину можна купити";
            }
            else
            {
                player.NeedPay = true;
                message = "Ви потрапили на територію чужої компанії. Необхідно сплатити рахунки";
            }

            await dbPlayer.UpdatePlayerAsync(player);
            return message;
        }
        private async Task<string> ActionNextPlayer(string gameId, string oldPlayerId)
        {
            List<Player> players = await dbPlayer.ReadPlayerListAsync(gameId);
            Player nextPlayer = players[players.FindIndex(p => p.Id == oldPlayerId)];

            while (true)
            {
                nextPlayer = players[(players.FindIndex(p => p.Name == nextPlayer.Name) + 1) % players.Count];
                if (nextPlayer.InGame == false) continue;

                if (nextPlayer.IsPrisoner == true)
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
        private async Task<bool> CheckMonopoly(Cell cell)
        {
            int monopolyIndex = Constants.CellsMonopolyIndex[cell.Number];
            List<Cell> cells = await dbCells.ReadCellListAsync(cell.GameId);

            List<Cell> monopolyCells = new List<Cell>();
            for (int i = 0; i < Constants.CellsMonopolyIndex.Count; i++)
            {
                if (Constants.CellsMonopolyIndex[i] == monopolyIndex)
                    monopolyCells.Add(cells[i]);
            }
            for (int i = 0; i < monopolyCells.Count; i++)
            {
                if (monopolyCells[i].Owner != cell.Owner)
                    return false;
            }
            return true;
        }

        private async Task<string?> ChekWinner(string gameId)
        {
            List<Player> players = await dbPlayer.ReadPlayerListAsync(gameId);
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
        private async Task DeleteGameAndRoom(string gameId)
        {
            var deleteRoomTask = dbRoom.DeleteRoomAsync(gameId);
            var deletePlayersInRoomTask = dbPlayerInRoom.DeleteAllPlayersInRoomAsync(gameId);
            var deleteCellsTask = dbCells.DeleteCellsAsync(gameId);
            var deletePlayersTask = dbPlayer.DeletePlayersAsync(gameId);

            await Task.WhenAll(deleteRoomTask, deletePlayersInRoomTask, deleteCellsTask, deletePlayersTask);
        }
    }
}
