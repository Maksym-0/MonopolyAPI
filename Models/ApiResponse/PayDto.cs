namespace Monopoly.Models.ApiResponse
{
    public class PayDto
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }

        public string? ReceiverId { get; set; }
        public string? ReceiverName { get; set; }
        
        public int NewPlayerBalance { get; set; }
        public int? NewReceiverBalance { get; set; }

        public int Amount { get; set; }
    }
}
