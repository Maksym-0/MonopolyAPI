namespace Monopoly.Models
{
    public class Player
    {
        public string Name { get; set; }
        public string GameId { get; set; }
        public int Balance { get; set; }
        public int Location { get; set; }
        public int CantAction { get; set; }
        public int ReverseMove { get; set; }
        public bool IsPrisoner { get; set; }
        public bool InGame { get; set; }
        public bool NeedPay { get; set; }
        public bool HisAction { get; set; }
        public bool CanMove { get; set; }
        public bool CanBuyCell { get; set; }
        public bool CanUpdateCell { get; set; }
        public Player(string gameId, string name)
        {
            Name = name;
            GameId = gameId;
            Balance = 3000;
            Location = 0;
            ReverseMove = 0;
            IsPrisoner = false;
            NeedPay = false;
            InGame = true;
            HisAction = false;
            CantAction = 0;
            CanMove = false;
            CanBuyCell = false;
            CanUpdateCell = false;
        }
        public Player() { }
        public void StartAction()
        {
            HisAction = true;
            CanMove = true;
            CanUpdateCell = true;
        }
        public void StopAction()
        {
            HisAction = false;
            NeedPay = false;
            CanMove = false;
            CanBuyCell = false;
            CanUpdateCell = false;
        }
        public bool CheckEndAction()
        {
            if (CanMove == false && CanBuyCell == false && CanUpdateCell == false && NeedPay == false) return true;
            else return false;
        }
        public bool Buy(int payment)
        {
            if (Balance >= payment)
            {
                Balance -= payment;
                return true;
            }
            else return false;
        }
        public bool PayRent(int payment)
        {
            if (Balance >= payment)
            {
                Balance -= payment;
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
                CanUpdateCell = false;
                return true;
            }
            else return false;
        }
        public bool PayToLeavePrison()
        {
            if (PayRent(300)) 
            {
                IsPrisoner = false;
                CantAction = 0;
                return true; 
            }
            else return false;
        }
    }
}
