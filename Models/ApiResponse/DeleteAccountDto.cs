namespace Monopoly.Models.ApiResponse
{
    public class DeleteAccountDto
    {
        public string? AccountId { get; set; }
        public string? Name { get; set; }

        public bool IsDeleted { get; set; }
    }
}
