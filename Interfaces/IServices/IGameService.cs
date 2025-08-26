using Monopoly.Models.ApiResponse;
using Monopoly.Models.GameModels;

namespace Monopoly.Interfaces.IServices
{
    public interface IGameService
    {
        Task<GameDto> StatusOfGameAsync(string gameId);
        Task<MoveDto> MoveAsync(string gameId, string playerName);
        Task<PayDto> TryPayAsync(string gameId, string playerName);
        Task<BuyDto> TryBuyAsync(string gameId, string playerName);
        Task<LevelChangeDto> LevelUpAsync(string gameId, string playerName, int cellNumber);
        Task<LevelChangeDto> LevelDownAsync(string gameId, string playerName, int cellNumber);
        Task<NextActionDto> EndActionAsync(string gameId, string playerName);
        Task<LeaveGameDto> LeaveGameAsync(string gameId, string payerName);
    }
}
