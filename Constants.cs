namespace Monopoly
{
    public static class Constants
    {
        public static readonly string Connect = Environment.GetEnvironmentVariable("DB_Connect") ?? "Host=localhost;UserName=postgres;Password=5973m4b0;Database=postgres";
        public static readonly string DBaccountName = Environment.GetEnvironmentVariable("DB_Account") ?? "AccountData";
        public static readonly string DBroomName = Environment.GetEnvironmentVariable("DB_Room") ?? "RoomData";
        public static readonly string DBplayerInRoomName = Environment.GetEnvironmentVariable("DB_PlayerinRoom") ?? "PlayerInRoomData";
        public static readonly string DBcellName = Environment.GetEnvironmentVariable("DB_Game") ?? "GameData";
        public static readonly string DBplayerName = Environment.GetEnvironmentVariable("DB_Player") ?? "PlayerData";
        public static readonly string JwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? "my_secret_key_1234567891011121314151617181920";

        public static readonly List<string> CellNames = new List<string>
        {
            "Старт", "Сільпо", "Мінус гроші", "Таврія В", "АТБ", "Швидка допомога", "Монобанк", "Пузата хата", "KFC", "Плюс гроші", "McDpnald`s",
            "В'язниця", "Sleeper ", "Marsego", "M-TAC", "Приватбанк", "Intel", "Плюс гроші", "AMD",
            "Казино", "Sony", "Мінус гроші", "Nintendo Switch", "Xbox", "Мінус гроші", "ПУМБ", "Київстар", "Vodafone", "Плюс гроші", "Lifecell",
            "Відпочинок", "Grammarly", "Reface", "MacPaw", "Ощадбанк", "Живчик", "Мінус гроші", "Моршинська"
        };
        public static readonly List<string> SpecialCellNames = new List<string>
        {
            "Старт", "В'язниця", "Казино", "Відпочинок", "Мінус гроші", "Плюс гроші", "Швидка допомога", "Зворотній хід", "Торнадо"
        };
        public static readonly List<int> CellsMonopolyIndex = new List<int>
        {
            0, 1, 0, 1, 1, 0, 2, 3, 3, 0, 3,
            0, 4, 4, 4, 2, 5, 0, 5,
            0, 6, 0, 6, 6, 0, 2, 7, 7, 0, 7,
            0, 8, 8, 8, 2, 9, 0, 9
        }; // Індекси для визначення клітин однієї монополії
        
        public static readonly List<int> CellPrices = new List<int>
        {
            0, 160, 0, 160, 160, 0, 500, 200, 200, 0, 200,
            0, 280, 280, 280, 500, 400, 0, 400,
            0, 480, 0, 480, 480, 0, 500, 520, 520, 0, 520,
            0, 700, 700, 700, 500, 900, 0, 900
        };

        public static readonly List<int> CellStartRents = new List<int>
        {
            0, 8, 0, 8, 8, 0, 100, 12, 12, 0, 12,
            0, 20, 20, 20, 100, 32, 0, 32,
            0, 36, 0, 36, 36, 0, 100, 44, 0, 44, 44,
            0, 56, 56, 56, 100, 90, 0, 90
        };
        public static readonly List<int> CellBaseMonopolyRents = new List<int>
        {
            0, 16, 0, 16, 16, 0, 200, 24, 24, 0, 24,
            0, 40, 40, 40, 200, 64, 0, 64,
            0, 72, 0, 72, 72, 0, 200, 88, 0, 88, 88,
            0, 112, 112, 112, 200, 180, 0, 180
        };
        
        public static readonly List<int> CellLevel1Rents = new List<int>
        {
            0, 40, 0, 40, 40, 0, 500, 60, 60, 0, 60,
            0, 100, 100, 100, 500, 160, 0, 160,
            0, 180, 0, 180, 180, 0, 500, 220, 0, 220, 220,
            0, 300, 300, 300, 500, 360, 0, 360
        };
        public static readonly List<int> CellLevel2Rents = new List<int>
        {
            0, 120, 0, 120, 120, 0, 1200, 180, 180, 0, 180,
            0, 300, 300, 300, 1200, 400, 0, 400,
            0, 500, 0, 500, 500, 0, 1200, 660, 0, 660, 660,
            0, 900, 900, 900, 1200, 1100, 0, 1100
        };
        public static readonly List<int> CellLevel3Rents = new List<int>
        {
            0, 360, 0, 360, 360, 0, 1800, 540, 540, 0, 540,
            0, 900, 900, 900, 1800, 1150, 0, 1150,
            0, 1400, 0, 1400, 1400, 0, 1800, 1600, 0, 1600, 1600,
            0, 2000, 2000, 2000, 1800, 2600, 0, 2600
        };
        public static readonly List<int> CellLevel4Rents = new List<int>
        {
            0, 640, 0, 640, 640, 0, 2600, 800, 800, 0, 800,
            0, 1250, 1250, 1250, 2600, 1550, 0, 1550,
            0, 1750, 0, 1750, 1750, 0, 2600, 1950, 0, 1950, 1950,
            0, 2500, 2500, 2500, 2600, 3100, 0, 3100
        };
        public static readonly List<int> CellLevel5Rents = new List<int>
        {
            0, 900, 0, 900, 900, 0, 3200, 1100, 1100, 0, 1100,
            0, 1500, 1500, 1500, 3200, 1900, 0, 1900,
            0, 2200, 0, 2200, 2200, 0, 3200, 2400, 0, 2400, 2400,
            0, 2900, 2900, 2900, 3200, 3500, 0, 3500
        };

        public static readonly List<int> CellBuildAndSellCost = new List<int>
        {
            0, 100, 0, 100, 100, 0, 500, 120, 120, 0, 120,
            0, 200, 200, 200, 500, 250, 0, 250,
            0, 300, 0, 300, 300, 0, 500, 300, 0, 300, 300,
            0, 350, 350, 350, 500, 400, 0, 400
        };
        public static readonly List<int> CellPledgeCost = new List<int>
        {
            0, 80, 0, 80, 80, 0, 300, 100, 100, 0, 100,
            0, 140, 140, 140, 300, 200, 0, 200,
            0, 240, 0, 240, 240, 0, 300, 260, 0, 260, 260,
            0, 350, 350, 350, 300, 450, 0, 450
        };
    }
}
