using Monopoly.Database;

namespace Monopoly.Models
{
    public class Cell
    {
        public string GameId { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public int? Price { get; set; }
        public int? Rent { get; set; }
        public string? OwnerName { get; set; }
        public int Level { get; set; }
        public Cell(string gameId, string name, int number)
        {
            GameId = gameId;
            Name = name;
            Number = number;
        }
        public Cell(string gameId, string name, int number, int price, int rent)
        {
            GameId = gameId;
            Name = name;
            Number = number;
            Price = price;
            Rent = rent;
            OwnerName = null;
            Level = 0;
        }
        public Cell() { }
        public bool ChangeCellLevel(int newLevel)
        {
            switch (newLevel)
            {
                case 0:
                    Rent = Constants.CellStartRents[Number];
                    Level = 0;
                    break;
                case 1:
                    Rent = Constants.CellLevel1Rents[Number];
                    Level = 1;
                    break;
                case 2:
                    Rent = Constants.CellLevel2Rents[Number];
                    Level = 2;
                    break;
                case 3:
                    Rent = Constants.CellLevel3Rents[Number];
                    Level = 3;
                    break;
                case 4:
                    Level = 4;
                    Rent = Constants.CellLevel4Rents[Number];
                    break;
                case 5:
                    Level = 5;
                    Rent = Constants.CellLevel5Rents[Number];
                    break;
            }
            return true;
        }
        public async Task<bool> CheckMonopoly()
        {
            int monopolyIndex = Constants.CellsMonopolyIndex[Number];
            DBCells dbCells = new DBCells();
            List<Cell> cells = await dbCells.ReadCellListAsync(GameId);

            List<Cell> monopolyCells = new List<Cell>();
            for(int i = 0; i < Constants.CellsMonopolyIndex.Count; i++) 
            {
                if (Constants.CellsMonopolyIndex[i] == monopolyIndex)
                    monopolyCells.Add(cells[i]);
            }
            for (int i = 0; i < monopolyCells.Count; i++)
            {
                if (monopolyCells[i].OwnerName != OwnerName)
                    return false;
            }
            return true;
        }
    }
}
