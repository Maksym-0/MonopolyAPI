using Monopoly.Models.RoomModels;

namespace Monopoly.Models.GameModels
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string GameId { get; set; }
        public int Balance { get; set; }
        public int Location { get; set; }
        public int CantAction { get; set; }
        public int ReverseMove { get; set; }
        public Dice LastDiceResult { get; set; }
        public bool IsPrisoner { get; set; }
        public bool InGame { get; set; }
        public bool NeedPay { get; set; }
        public bool HisAction { get; set; }
        public bool CanMove { get; set; }
        public bool CanBuyCell { get; set; }
        public bool CanLevelUpCell { get; set; }
        public Player(PlayerInRoom basicPlayer)
        {
            Id = basicPlayer.Id;
            Name = basicPlayer.Name;
            GameId = basicPlayer.RoomId;
            Balance = 3000;
            Location = 0;
            ReverseMove = 0;
            LastDiceResult = new Dice(0, 0);
            IsPrisoner = false;
            NeedPay = false;
            InGame = true;
            HisAction = false;
            CantAction = 0;
            CanMove = false;
            CanBuyCell = false;
            CanLevelUpCell = false;
        }
        public Player(string gameId, string playerId, string name)
        {
            Id = playerId;
            Name = name;
            GameId = gameId;
            Balance = 3000;
            Location = 0;
            ReverseMove = 0;
            LastDiceResult = new Dice(0, 0);
            IsPrisoner = false;
            NeedPay = false;
            InGame = true;
            HisAction = false;
            CantAction = 0;
            CanMove = false;
            CanBuyCell = false;
            CanLevelUpCell = false;
        }
        public Player() { }
        public void StartAction()
        {
            HisAction = true;
            CanMove = true;
            CanLevelUpCell = true;
        }
        public void StopAction()
        {
            HisAction = false;
            NeedPay = false;
            CanMove = false;
            CanBuyCell = false;
            CanLevelUpCell = false;
        }
        public bool CheckEndAction()
        {
            if (CanMove == false && CanBuyCell == false && CanLevelUpCell == false && NeedPay == false) return true;
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
