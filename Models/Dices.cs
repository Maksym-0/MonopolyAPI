namespace Monopoly.Models
{
    public class Dice
    {
        public int Dice1 { get; set; }
        public int Dice2 {  get; set; }
        public int DiceSum {  get; set; }
        public bool Dubl { get; set; }
        public Dice()
        {
            Random random = new Random();

            Dice1 = random.Next(1, 7);
            Dice2 = random.Next(1, 7);

            if (Dice1 == Dice2)
                Dubl = true;
            else
                Dubl = false;
            DiceSum = Dice1 + Dice2;
        }
    }
}
