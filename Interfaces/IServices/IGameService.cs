using Monopoly.Database;
using Monopoly.Models.APIResponse;

namespace Monopoly.Interfaces.IServices
{
    public interface IGameService
    {
        Task<GameReponse> StatusOfGameAsync(string gameId);
        Task<string> MoveAsync(string gameId, string playerName);
        Task<bool> TryPayAsync(string gameId, string playerName);
        Task<bool> TryBuyAsync(string gameId, string playerName);
        Task LevelUpAsync(string gameId, string playerName, int cellNumber);
        Task LevelDownAsync(string gameId, string playerName, int cellNumber);
        Task<string> EndActionAsync(string gameId, string playerName);
        Task<string> LeaveGameAsync(string gameId, string payerName);
    }
}
