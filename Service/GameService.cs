using Monopoly;
using Monopoly.Models;
using Monopoly.Database;
using System.Numerics;
using Microsoft.AspNetCore.Mvc;
namespace Monopoly.Service
{
    public class GameService
    {
        public async Task<string?> ValidateMoveAsync(string gameId, string name)
        {
            DBPlayerStatus dbPlayer = new DBPlayerStatus();
            Player player = await dbPlayer.ReadPlayerAsync(gameId, name);

            if (!player.InGame)
                return "Гравець поза грою";
            else if (!player.HisAction)
                return "Гравець не може ходити не в свій хід";
            else if (!player.CanMove)
                return "Гравець більше не може кидати кубики та рухатись";
            return null;
        }
        public async Task<string?> ValidatePayAsync(string gameId, string name)
        {
            DBPlayerStatus dbPlayer = new DBPlayerStatus();
            Player player = await dbPlayer.ReadPlayerAsync(gameId, name);

            if (!player.InGame)
                return "Гравець поза грою";
            else if (!player.NeedPay)
                return "Гравець не повинен платити";
            return null;
        }
        public async Task<string?> ValidateBuyAsync(string gameId, string name)
        {
            DBCells dbCells = new DBCells();
            DBPlayerStatus dbPlayer = new DBPlayerStatus();
            
            Player player = await dbPlayer.ReadPlayerAsync(gameId, name);
            Cell cell = await dbCells.ReadCellAsync(gameId, player.Location);

            if (!player.InGame)
                return "Гравець поза грою";
            else if (!player.CanBuyCell)
                return "Гравець не може придбати клітину";
            else if (cell.OwnerName != null)
                return "Неможливо придбати клітину, що належить іншому гравцю";
                return null;
        }
        public async Task<string?> ValidateLevelUpAsync(string gameId, string name, int cellNumber)
        {
            DBPlayerStatus dbPlayer = new DBPlayerStatus();
            DBCells dbCells = new DBCells();

            Player player = await dbPlayer.ReadPlayerAsync(gameId, name);
            Cell cell = await dbCells.ReadCellAsync(gameId, cellNumber);

            if (!player.InGame)
                return "Гравець поза грою";
            else if (!player.CanLevelUpCell)
                return "Гравець не може підняти рівень клітини";
            if (Constants.SpecialCellNames.Contains(Constants.CellNames[cellNumber]))
                return "Неможливо змінити рівень особливої клітини";
            else if (!await cell.CheckMonopoly())
                return "Відсутня монополія, підняти рівень клітини неможливо";
            else if (cell.OwnerName != name)
                return "Заборонено змінювти рівень клітини, що Вам не належить";
            else if (cell.Level == 5)
                return "Рівень обраної клітини вже є максимальний";
            else if (player.Balance < Constants.CellBuildAndSellCost[cellNumber])
                return "Вам не стане коштів на це будівництво";
            return null;
        }
        public async Task<string?> ValidateLevelDownAsync(string gameId, string name, int cellNumber)
        {
            DBPlayerStatus dbPlayer = new DBPlayerStatus();
            DBCells dbCells = new DBCells();

            Player player = await dbPlayer.ReadPlayerAsync(gameId, name);
            Cell cell = await dbCells.ReadCellAsync(gameId, cellNumber);

            if (!player.InGame)
                return "Гравець поза грою";
            else if (!player.HisAction)
                return "Гравець не може змінювати рівень клітин не в свій хід";
            else if (cell.OwnerName != name)
                return "Заборонено змінювти рівень клітини, що Вам не належить";
            else if (cell.Level == 0)
                return "Рівень обраної клітини вже є мінімальний";
            return null;
        }
        public async Task<string?> ValidateEndActionAsync(string gameId, string name)
        {
            DBPlayerStatus dbPlayer = new DBPlayerStatus();
            Player player = await dbPlayer.ReadPlayerAsync(gameId, name);

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
        public async Task<string?> ValidateLeaveAsync(string gameId, string name)
        {
            DBPlayerStatus dbPlayer = new DBPlayerStatus();
            Player player = await dbPlayer.ReadPlayerAsync(gameId, name);

            if (!player.InGame)
                return "Гравець вже поза грою";
            return null;
        }

        public async Task<GameStatus> StatusOfGameAsync(string gameId)
        {
            DBCells dbCells = new DBCells();
            DBPlayerStatus dbPlayer = new DBPlayerStatus();

            List<Player> players = new List<Player>();
            List<Cell> cells = new List<Cell>();

            cells = await dbCells.ReadCellListAsync(gameId);
            players = await dbPlayer.ReadPlayerListAsync(gameId);

            GameStatus gameStatus = new GameStatus(gameId, cells, players);

            return gameStatus;
        }
        public async Task<string> MoveAsync(string gameId, string playerName)
        {
            DBCells dbCells = new DBCells();
            DBPlayerStatus dbPlayer = new DBPlayerStatus();

            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerName);

            List<Player> players = new List<Player>();
            List<Cell> cells = new List<Cell>();
            cells = await dbCells.ReadCellListAsync(gameId);
            players = await dbPlayer.ReadPlayerListAsync(gameId);

            Dice dice = new Dice();

            player.CanMove = false;

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
        public async Task<string?> TryPayAsync(string gameId, string playerName)
        {
            DBCells dbCells = new DBCells();
            DBPlayerStatus dbPlayer = new DBPlayerStatus();

            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerName);
            Cell cell = await dbCells.ReadCellAsync(gameId, player.Location);

            if (Constants.SpecialCellNames.Contains(cell.Name))
            {
                if (player.IsPrisoner == true)
                {
                    if (player.PayToLeavePrison())
                    {
                        await dbPlayer.UpdatePlayerAsync(player);
                        return "Ви покинули в'язницю!";
                    }
                    return null;
                }
            }
            if (player.Balance < cell.Rent)
            {
                return null;
            }
            else
            {
                player.PayRent(Convert.ToInt32(cell.Rent));
                await dbPlayer.UpdatePlayerAsync(player);
                return "Оплату проведено";
            }
        }
        public async Task<string?> TryBuyAsync(string gameId, string playerName)
        {
            DBCells dbCells = new DBCells();
            DBPlayerStatus dbPlayer = new DBPlayerStatus();

            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerName);
            Cell cell = await dbCells.ReadCellAsync(gameId, player.Location);
            
            if (player.Balance >= cell.Price)
            {
                player.Buy(Convert.ToInt32(cell.Price));
                cell.OwnerName = player.Name;
                await dbPlayer.UpdatePlayerAsync(player);
                await dbCells.UpdateCellAsync(cell);
                return "Клітину придбано";
            }
            else return null;
        }
        public async Task<string> LevelUpAsync(string gameId, string playerName, int cellNumber)
        {
            DBCells dbCells = new DBCells();
            DBPlayerStatus dbPlayer = new DBPlayerStatus();

            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerName);
            Cell cell = await dbCells.ReadCellAsync(gameId, cellNumber);

            player.PayToUpgrade(Constants.CellBuildAndSellCost[cellNumber]);
            cell.ChangeCellLevel(cell.Level + 1);
            await dbPlayer.UpdatePlayerAsync(player);
            await dbCells.UpdateCellAsync(cell);
            return "Рівень клітини піднято";
        }
        public async Task<string> LevelDownAsync(string gameId, string playerName, int cellNumber)
        {
            DBCells dbCells = new DBCells();
            DBPlayerStatus dbPlayer = new DBPlayerStatus();

            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerName);
            Cell cell = await dbCells.ReadCellAsync(gameId, cellNumber);

            player.Balance += Constants.CellBuildAndSellCost[cellNumber];
            cell.ChangeCellLevel(cell.Level - 1);
            await dbPlayer.UpdatePlayerAsync(player);
            await dbCells.UpdateCellAsync(cell);
            return "Рівень клітини знижено";
        }
        public async Task<string> EndActionAsync(string gameId, string playerName)
        {
            DBPlayerStatus dbPlayer = new DBPlayerStatus();

            Player player = await dbPlayer.ReadPlayerAsync(gameId, playerName);

            player.StopAction();
            await dbPlayer.UpdatePlayerAsync(player);
            string newPlayer = await ActionNextPlayer(gameId, playerName);
            return $"Хід гравця завершено та передано {newPlayer}";
        }
        public async Task<string> LeaveGameAsync(string gameId, string payerName)
        {
            DBCells dbCells = new DBCells();
            DBPlayerStatus dbPlayer = new DBPlayerStatus();

            Player player = await dbPlayer.ReadPlayerAsync(gameId, payerName);
            player.StopAction();
            player.InGame = false;

            List<Cell> cells = await dbCells.ReadCellListAsync(gameId);

            for (int i = 0; i < cells.Count; i++)
            {
                Cell cell = cells[i];
                if (cell.OwnerName == payerName)
                {
                    cell.Price = Constants.CellPrices[i];
                    cell.Rent = Constants.CellStartRents[i];
                    cell.OwnerName = null;
                    cell.Level = 0;
                    await dbCells.UpdateCellAsync(cell);
                }
            }
            string? winner = await ChekWin(gameId);
            if (winner != null)
            {
                DeleteGameInMinutes(gameId);
                return $"Гравець покинув гру. Переможець: {winner}";
            }
            return "Гравець покинув гру";
        }

        private async Task<string> CellEffectAsync(Player player, List<Cell> cells)
        {
            DBPlayerStatus dbPlayer = new DBPlayerStatus();
            
            Cell cell = cells[player.Location];
            if (Constants.SpecialCellNames.Contains(cell.Name))
            {
                Random random = new Random();
                switch (cell.Name)
                {
                    case "Старт":
                        player.Balance += 300;
                        await dbPlayer.UpdatePlayerAsync(player);
                        return "Ви потрапили на клітину Старт. Отримали гроші";
                    case "В'язниця":
                        player.Location = 30;
                        await dbPlayer.UpdatePlayerAsync(player);
                        return "Ви потрапили на клітину В'язниця. Їдьте на відпочинок";
                    case "Казино":
                        int win = random.Next(-3, 4) * 100;
                        player.Balance += win;
                        await dbPlayer.UpdatePlayerAsync(player);
                        return $"Ви потрапили на клітину Казино. Ваш виграш: {win}";
                    case "Відпочинок":
                        player.Location = 11;
                        player.CantAction = 3;
                        player.IsPrisoner = true;
                        await dbPlayer.UpdatePlayerAsync(player);
                        return "Ви потрапили на клітину Відпочинок. Ви потрапили до тюрми";
                    case "Мінус гроші":
                        int lose = random.Next(-3, 0) * 100;
                        player.Balance += lose;
                        await dbPlayer.UpdatePlayerAsync(player);
                        return $"Ви потрапили на клітину Мінус гроші. Ви втратили {lose}";
                    case "Плюс гроші":
                        int plus = random.Next(1, 4) * 100;
                        player.Balance += plus;
                        await dbPlayer.UpdatePlayerAsync(player);
                        return $"Ви потрапили на клітину Плюс гроші. Ви отримали {plus}";
                    case "Швидка допомога":
                        player.CantAction = 1;
                        await dbPlayer.UpdatePlayerAsync(player);
                        return "Ви потрапили на клітину Швидка допомога. Пропустіть один хід";
                    case "Зворотній хід":
                        player.ReverseMove = 1;
                        await dbPlayer.UpdatePlayerAsync(player);
                        return "Ви потрапили на клітину Зворотній хід. Наступний хід відбудеться у зворотньому напрямку";
                    case "Торнадо":
                        return "Можливість не реалізовано";
                    default:
                        return "Невідома особлива клітина";
                }
            }
            else if (cell.OwnerName == null)
            {
                player.CanBuyCell = true;
                await dbPlayer.UpdatePlayerAsync(player);
                return "Рух завершено. Клітину можна купити";
            }
            else
            {
                player.NeedPay = true;
                await dbPlayer.UpdatePlayerAsync(player);
                return "Ви потрапили на територію чужої компанії. Необхідно сплатити рахунки";
            }
        }
        private async Task<string> ActionNextPlayer(string gameId, string oldPlayerName)
        {
            DBPlayerStatus dbPlayer = new DBPlayerStatus();
            List<Player> players = await dbPlayer.ReadPlayerListAsync(gameId);
            Player nextPlayer = players[players.FindIndex(p => p.Name == oldPlayerName)];
            while (true)
            {
                nextPlayer = players[(players.FindIndex(p => p.Name == nextPlayer.Name) + 1) % players.Count];

                if (nextPlayer.InGame == false) continue;
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
        private async Task<string?> ChekWin(string gameId)
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
        private async Task DeleteGameInMinutes(string gameId)
        {
            DBPlayerStatus dbPlayer = new DBPlayerStatus();
            DBCells dbCells = new DBCells();
            DBRoom dbRoom = new DBRoom();
            dbRoom.DeleteRoomAsync(gameId);
            await Task.Delay(TimeSpan.FromMinutes(3));

            dbCells.DeleteCellsAsync(gameId);
            dbPlayer.DeletePlayersAsync(gameId);
        }
    }
}
