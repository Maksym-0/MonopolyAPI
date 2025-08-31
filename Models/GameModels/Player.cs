using Monopoly.Models.RoomModels;

namespace Monopoly.Models.GameModels
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string GameId { get; set; }
        public int Balance { get; set; } = 3000;
        public int Location { get; set; } = 0;
        public int CantAction { get; set; } = 0;
        public int ReverseMove { get; set; } = 0;
        public Dice LastDiceResult { get; set; } = new Dice(0, 0);
        public int CountOfDubles { get; set; } = 0;
        public bool IsPrisoner { get; set; } = false;
        public bool InGame { get; set; } = true;
        public bool NeedPay { get; set; } = false;
        public bool HisAction { get; set; } = false;
        public bool CanRollDice { get; set; } = false;
        public bool CanBuyCell { get; set; } = false;
        public bool CanLevelUpCell { get; set; } = false;
        public Player(PlayerInRoom basicPlayer)
        {
            Id = basicPlayer.Id;
            Name = basicPlayer.Name;
            GameId = basicPlayer.RoomId;
        }
        public Player(string gameId, string playerId, string name)
        {
            Id = playerId;
            Name = name;
            GameId = gameId;
        }
        public Player() { }
        public void StartAction()
        {
            HisAction = true;
            CanRollDice = true;
            CanLevelUpCell = true;
        }
        public void StartPrisonerAction()
        {
            HisAction = true;
            CanRollDice = true;
            CanLevelUpCell = true;
        }
        public void StopAction()
        {
            HisAction = false;
            NeedPay = false;
            CanRollDice = false;
            CanBuyCell = false;
            CanLevelUpCell = false;
        }
        public bool CheckEndAction()
        {
            if (CanRollDice == false && CanBuyCell == false && CanLevelUpCell == false && NeedPay == false) return true;
            else return false;
        }
        public bool DeductMoney(int amount)
        {
            if (Balance >= amount)
            {
                Balance -= amount;
                return true;
            }
            else return false;
        }
        public bool PayRent(Player receiver, int payment)
        {
            if (Balance >= payment)
            {
                Balance -= payment;
                receiver.Balance += payment;
                NeedPay = false;
                return true;
            }
            else return false;
        }
        public bool PayToUpgrade(int payment)
        {
            if (Balance >= payment)
            {
                Balance -= payment;
                CanLevelUpCell = false;
                return true;
            }
            else return false;
        }
        public bool PayToLeavePrison(int leavePrisonCost)
        {
            if (Balance >= leavePrisonCost)
            {
                Balance -= leavePrisonCost;
                IsPrisoner = false;
                CantAction = 0;
                return true;
            }
            else return false;
        }
    }
}
