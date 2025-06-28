using Monopoly.Models;

namespace Monopoly.Abstractions
{
    public interface ICellRepository
    {
        Task InsertCellsAsync(List<Cell> cells);
        Task<List<Cell>> ReadCellListAsync(string gameId);
        Task<Cell> ReadCellAsync(string gameId, int cellNumber);
        Task UpdateCellAsync(Cell cell);
        Task DeleteCellsAsync(string gameId);
    }
}
