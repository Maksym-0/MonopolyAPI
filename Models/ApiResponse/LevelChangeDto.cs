namespace Monopoly.Models.ApiResponse
{
    public class LevelChangeDto
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        
        public int CellNumber { get; set; }
        public string CellName { get; set; }
        
        public int OldLevel { get; set; }
        public int NewLevel { get; set; }

        public int OldPlayerBalance { get; set; }
        public int NewPlayerBalance { get; set; }
    }
}
