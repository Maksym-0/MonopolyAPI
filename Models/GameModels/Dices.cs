namespace Monopoly.Models.GameModels
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
        public Dice(int dice1, int dice2)
        {
            Dice1 = dice1;
            Dice2 = dice2;
            Dubl = (dice1 == dice2);
            DiceSum = dice1 + dice2;
        }
    }
}
