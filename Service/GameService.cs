using Monopoly.Interfaces.IServices;
using Monopoly.Interfaces.IDatabases;
using Monopoly.Models.GameModels;
using Monopoly.Models.ApiResponse;
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

        public async Task<GameDto> StatusOfGameAsync(string gameId)
        {
            List<Player> players = await dbPlayer.ReadPlayerListAsync(gameId);
            List<Cell> cells = await dbCells.ReadCellListAsync(gameId);

            GameDto gameStatus = new GameDto(gameId, cells, players);

            return gameStatus;
        }
        public async Task<MoveDto> MoveAsync(string gameId, string playerId)
        {
            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);

            string? isValid =  ValidateMove(player);
            if (isValid != null)
                throw new Exception(isValid);
            
            List<Cell> cells = await dbCells.ReadCellListAsync(gameId);

            Dice dice = new Dice();

            player.CanRollDice = false;
            player.LastDiceResult = dice;

            if (player.LastDiceResult.Dubl)
            {
                if (player.IsPrisoner)
                {
                    player.IsPrisoner = false;
                    player.CantAction = 0;
                    player.CountOfDubles = 0;

                    await dbPlayer.UpdatePlayerAsync(player);
                    return new MoveDto()
                    {
                        Player = player,
                        Cell = cells[player.Location],
                        CellMessage = $"{player.Name} викинув дубль і виходить з в'язниці"
                    };
                }
                else
                {
                    player.CountOfDubles += 1;
                    if (player.CountOfDubles == 3)
                    {
                        player.Location = 30;
                        player.IsPrisoner = true;
                        player.CantAction = 3;
                        player.CountOfDubles = 0;
                        player.StopAction();
                        await dbPlayer.UpdatePlayerAsync(player);
                        return new MoveDto()
                        {
                            Player = player,
                            Cell = cells[player.Location],
                            CellMessage = $"{player.Name} тричі поспіль викинув дубль і відправляється до в'язниці"
                        };
                    }
                }
            }
            else
            {
                player.CountOfDubles = 0;
                if (player.IsPrisoner)
                {
                    await dbPlayer.UpdatePlayerAsync(player);
                    return new MoveDto()
                    {
                        Player = player,
                        Cell = cells[player.Location],
                        CellMessage = $"{player.Name} не викинув дубль і залишається у в'язниці"
                    };
                }
            }

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

            if (player.LastDiceResult.Dubl)
            {
                player.StartAction();
            }

            Cell cell = cells[player.Location];
            string cellMessage = await CellEffectAsync(player, cell);
            await dbPlayer.UpdatePlayerAsync(player);

            return new MoveDto()
            {
                Player = player,
                Cell = cell,
                CellMessage = cellMessage
            };
        }
        public async Task<PayDto> TryPayAsync(string gameId, string playerId)
        {
            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);
            Cell cell = await dbCells.ReadCellAsync(gameId, player.Location);

            string? isValid = ValidatePay(player, cell);
            if (isValid != null)
                throw new Exception(isValid);

            PayDto payResponse = new PayDto()
            {
                PlayerId = player.Id,
                PlayerName = player.Name,
                ReceiverId = null,
                ReceiverName = null,
                Amount = 0,
            };

            if (player.NeedPay)
            {
                List<Task> tasks = new List<Task>();
                Player receiver = await dbPlayer.ReadPlayerAsync(gameId, cell.OwnerId);

                player.PayRent(receiver, cell.Rent.Value);

                tasks.Add(dbPlayer.UpdatePlayerAsync(player));
                tasks.Add(dbPlayer.UpdatePlayerAsync(receiver));

                payResponse.NewPlayerBalance = player.Balance;
                payResponse.Amount = cell.Rent.Value;
                payResponse.ReceiverId = receiver.Id;
                payResponse.ReceiverName = receiver.Name;
            }
            else if (player.IsPrisoner)
            {
                player.PayToLeavePrison(Constants.LeavePrisonCost);
                await dbPlayer.UpdatePlayerAsync(player);

                payResponse.NewPlayerBalance = player.Balance;
                payResponse.Amount = Constants.LeavePrisonCost;
            }

            return payResponse;
        }
        public async Task<BuyDto> TryBuyAsync(string gameId, string playerId)
        {
            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);
            Cell cell = await dbCells.ReadCellAsync(gameId, player.Location);

            string? isValid = ValidateBuy(player, cell);
            if (isValid != null)
                throw new Exception(isValid);
            
            player.DeductMoney(Convert.ToInt32(cell.Price));
            cell.OwnerId = player.Id;
            
            List<Task> tasks = new List<Task>();
            tasks.Add(dbPlayer.UpdatePlayerAsync(player));
            
            bool hasMonopoly = await CheckAndApplyMonopoly(cell);
            if(!hasMonopoly)
                tasks.Add(dbCells.UpdateCellAsync(cell));
            
            BuyDto response = new BuyDto()
            {
                PlayerId = player.Id,
                PlayerName = player.Name,
                OldBalance = player.Balance + cell.Price.Value,
                NewBalance = player.Balance,
                
                CellNumber = cell.Number,
                CellName = cell.Name,
                CellMonopolyType = cell.MonopolyType,
                Price = cell.Price.Value,

                HasMonopoly = hasMonopoly
            };

            await Task.WhenAll(tasks);
            return response;
        }
        public async Task<LevelChangeDto> LevelUpAsync(string gameId, string playerId, int cellNumber)
        {
            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);
            Cell cell = await dbCells.ReadCellAsync(gameId, cellNumber);

            string? isValid =  ValidateLevelUpAsync(player, cell);
            if (isValid != null)
                throw new Exception(isValid);

            int oldBalance = player.Balance;
            player.PayToUpgrade(Constants.CellBuildAndSellCost[cellNumber]);
            cell.ChangeCellLevel(cell.Level + 1);

            List<Task> tasks = new List<Task>();

            tasks.Add(dbPlayer.UpdatePlayerAsync(player));
            tasks.Add(dbCells.UpdateCellAsync(cell));

            LevelChangeDto response = new LevelChangeDto()
            {
                PlayerId = player.Id,
                PlayerName = player.Name,

                CellNumber = cell.Number,
                CellName = cell.Name,

                NewLevel = cell.Level,
                OldLevel = cell.Level - 1,

                OldPlayerBalance = oldBalance,
                NewPlayerBalance = player.Balance
            };

            await Task.WhenAll(tasks);
            return response;
        }
        public async Task<LevelChangeDto> LevelDownAsync(string gameId, string playerId, int cellNumber)
        {
            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);
            Cell cell = await dbCells.ReadCellAsync(gameId, cellNumber);

            string? isValid = ValidateLevelDown(player, cell);
            if (isValid != null)
                throw new Exception(isValid);

            int oldBalance = player.Balance;
            player.Balance += Constants.CellBuildAndSellCost[cellNumber];
            cell.ChangeCellLevel(cell.Level - 1);
            
            List<Task> tasks = new List<Task>();

            tasks.Add(dbPlayer.UpdatePlayerAsync(player));
            tasks.Add(dbCells.UpdateCellAsync(cell));

            LevelChangeDto response = new LevelChangeDto()
            {
                PlayerId = player.Id,
                PlayerName = player.Name,

                CellNumber = cell.Number,
                CellName = cell.Name,

                NewLevel = cell.Level,
                OldLevel = cell.Level + 1,

                OldPlayerBalance = oldBalance,
                NewPlayerBalance = player.Balance
            };

            await Task.WhenAll(tasks);
            return response;
        }
        public async Task<NextActionDto> EndActionAsync(string gameId, string playerId)
        {
            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);

            string? isValid = ValidateEndAction(player);
            if (isValid != null)
                throw new Exception(isValid);

            player.StopAction();
            var updateTask = dbPlayer.UpdatePlayerAsync(player);
            Player newPlayer = await ActionNextPlayer(gameId, playerId);

            NextActionDto response = new NextActionDto()
            {
                PlayerId = player.Id,
                PlayerName = player.Name,

                NewPlayerId = newPlayer.Id,
                NewPlayerName = newPlayer.Name
            };

            await Task.WhenAll(updateTask);
            return response;
        }
        public async Task<LeaveGameDto> LeaveGameAsync(string gameId, string playerId)
        {
            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerId);

            string? isValid = ValidateLeave(player);
            if (isValid != null)
                throw new Exception(isValid);

            var room = await dbRoom.ReadRoomAsync(gameId);
            List<Task> tasks = new List<Task>();

            player.StopAction();
            player.InGame = false;
            room.CountOfPlayers -= 1;
            
            tasks.Add(dbPlayer.UpdatePlayerAsync(player));
            tasks.Add(dbPlayerInRoom.DeletePlayerInRoomAsync(playerId));
            tasks.Add(dbRoom.UpdateRoomAsync(room));

            List<Cell> cells = await dbCells.ReadCellListAsync(gameId);

            for (int i = 0; i < cells.Count; i++)
            {
                Cell cell = cells[i];
                if (cell.OwnerId == playerId)
                {
                    cell.Price = Constants.CellPrices[i];
                    cell.Rent = Constants.CellStartRents[i];
                    cell.OwnerId = null;
                    cell.Level = 0;
                    tasks.Add(dbCells.UpdateCellAsync(cell));
                }
            }
            await Task.WhenAll(tasks);
            Player? winner = await ChekWinner(gameId);

            LeaveGameDto leaveGameDto = new LeaveGameDto()
            {
                PlayerId = player.Id,
                PlayerName = player.Name,
                
                RemainingPlayers = room.CountOfPlayers,
                Winner = winner,

                IsGameOver = winner != null
            };

            if (winner != null)
            {
                await DeleteGameAndRoom(gameId);
                return leaveGameDto;
            }
            return leaveGameDto;
        }

        private string? ValidateMove(Player player)
        {
            if (!player.InGame)
                return "Гравець поза грою";
            else if (!player.HisAction)
                return "Гравець не може ходити не в свій хід";
            else if (!player.CanRollDice)
                return "Гравець більше не може кидати кубики";
            return null;
        }
        private string? ValidatePay(Player player, Cell cell)
        {
            if (!player.InGame)
                return "Гравець поза грою";
            else if (player.IsPrisoner && player.Balance < Constants.LeavePrisonCost)
                return "Недостатньо коштів для виходу з в'язниці";
            else if (!player.IsPrisoner && player.Balance < cell.Rent) 
                return "Недостатньо коштів для сплати ренти";
            else if (!player.NeedPay)
                return "Гравець не повинен платити";
            return null;
        }
        private string? ValidateBuy(Player player, Cell cell)
        {
            if (!player.InGame)
                return "Гравець поза грою";
            else if (cell.Unique)
                return "Неможливо придбати особливу клітину";
            else if (!player.CanBuyCell)
                return "Гравець не може придбати клітину";
            else if (cell.OwnerId != null)
                return "Неможливо придбати клітину, що належить іншому гравцю";
            else if (player.Balance < cell.Price)
                return "Неможливо придбати клітину. Недостатньо коштів";
            return null;
        }
        private string? ValidateLevelUpAsync(Player player, Cell cell)
        {
            if (!player.InGame)
                return "Гравець поза грою";
            else if (!player.CanLevelUpCell)
                return "Гравець не може підняти рівень клітини";
            else if (cell.Unique)
                return "Неможливо змінити рівень особливої клітини";
            else if (!cell.IsMonopoly.Value)
                return "Відсутня монополія, підняти рівень клітини неможливо";
            else if (cell.OwnerId != player.Id)
                return "Заборонено змінювти рівень клітини, що Вам не належить";
            else if (cell.Level == 5)
                return "Рівень обраної клітини вже є максимальний";
            else if (player.Balance < Constants.CellBuildAndSellCost[cell.Number])
                return "Вам не стане коштів на це будівництво";
            return null;
        }
        private string? ValidateLevelDown(Player player, Cell cell)
        {
            if (!player.InGame)
                return "Гравець поза грою";
            else if(cell.Unique)
                return "Неможливо знизити рівень особливої клітини";
            else if (!player.HisAction)
                return "Гравець не може змінювати рівень клітин не в свій хід";
            else if (cell.OwnerId != player.Id)
                return "Заборонено змінювти рівень клітини, що Вам не належить";
            else if (cell.Level == 0)
                return "Рівень обраної клітини вже є мінімальний";
            return null;
        }
        private string? ValidateEndAction(Player player)
        {
            if (!player.InGame)
                return "Гравець вже поза грою";
            else if (!player.HisAction)
                return "Гравець не може завершити нерозпочатий хід";
            else if (player.CanRollDice)
                return "Гравець не може завершити хід, не кинувши кубики";
            else if (player.NeedPay)
                return "Гравець не може завершити хід, не оплативши рахунки";
            return null;
        }
        private string? ValidateLeave(Player player)
        {
            if (!player.InGame)
                return "Гравець вже поза грою";
            return null;
        }

        private async Task<string> CellEffectAsync(Player player, Cell cell)
        {
            string message;

            if (cell.Unique)
            {
                switch (cell.Name)
                {
                    case "Старт":
                        player.Balance += 300;
                        message = $"{player.Name} потрапив на клітину Старт і отримав 300$";
                        break;
                    case "В'язниця":
                        player.Location = 30;
                        message = $"{player.Name} потрапив на клітину В'язниця і відправляється на відпочинок";
                        break;
                    case "Казино":
                        int win = random.Next(-3, 4) * 100;
                        player.Balance += win;
                        message = $"{player.Name} зайшов у Казино. Результат: {win}";
                        break;
                    case "Відпочинок":
                        player.Location = 11;
                        player.CantAction = 3;
                        player.IsPrisoner = true;
                        message = $"{player.Name} потрапив на клітину Відпочинок і йде до тюрми";
                        break;
                    case "Мінус гроші":
                        int lose = random.Next(-3, 0) * 100;
                        player.Balance += lose;
                        message = $"{player.Name} потрапив на клітину Мінус гроші і отримав {lose}";
                        break;
                    case "Плюс гроші":
                        int plus = random.Next(1, 4) * 100;
                        player.Balance += plus;
                        message = $"{player.Name} потрапив на клітину Плюс гроші і отримав {plus}";
                        break;
                    case "Швидка допомога":
                        player.CantAction = 1;
                        message = $"{player.Name} потрапив на клітину Швидка допомога і пропускає один хід";
                        break;
                    case "Зворотній хід":
                        player.ReverseMove = 1;
                        message = $"{player.Name} потрапив на клітину Зворотній хід. Наступний хід відбудеться у зворотньому напрямку";
                        break;
                    default:
                        message = $"{player.Name} потрапив на невідому особливу клітину";
                        break;
                }
            }
            else if (cell.OwnerId == null)
            {
                player.CanBuyCell = true;
                message = $"{player.Name} завершив рух. Клітину можна купити";
            }
            else
            {
                player.NeedPay = true;
                message = $"{player.Name} потрапив на територію чужої компанії. До сплати {cell.Rent}$";
            }

            return message;
        }
        private async Task<Player> ActionNextPlayer(string gameId, string oldPlayerId)
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
                    }
                    else
                    {
                        nextPlayer.CantAction--;
                        nextPlayer.StartPrisonerAction();
                    }
                }
                else if (nextPlayer.CantAction > 0)
                {
                    nextPlayer.CantAction--;
                }
                else
                {
                    nextPlayer.StartAction();
                }
                
                await dbPlayer.UpdatePlayerAsync(nextPlayer);
                return nextPlayer;
            }
        }
        private async Task<bool> CheckAndApplyMonopoly(Cell cell)
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
                if (monopolyCells[i].Number == cell.Number)
                {
                    continue;
                }
                if (monopolyCells[i].OwnerId != cell.OwnerId)
                {
                    return false;
                }
            }

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < monopolyCells.Count; i++)
            {
                monopolyCells[i].IsMonopoly = true;
                monopolyCells[i].Rent = Constants.CellBaseMonopolyRents[monopolyCells[i].Number];
                tasks.Add(dbCells.UpdateCellAsync(monopolyCells[i]));
            }

            await Task.WhenAll(tasks);
            return true;
        }

        private async Task<Player?> ChekWinner(string gameId)
        {
            List<Player> players = await dbPlayer.ReadPlayerListAsync(gameId);
            int playersInGame = 0;
            Player? winner = null;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].InGame)
                {
                    playersInGame++;
                    winner = players[i];
                }
            }
            if(playersInGame ==1)
                return winner;
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
