﻿using Monopoly.Models.GameModels;
using Npgsql;
using System.Data.Common;

namespace Monopoly.Interfaces.IDatabases
{
    public interface IPlayerRepository
    {
        Task InsertPlayersAsync(List<Player> players);
        Task<List<Player>> ReadPlayerListAsync(string gameId);
        Task<Player> ReadPlayerAsync(string gameId, string name);
        Task UpdatePlayerAsync(Player player);
        Task DeletePlayersAsync(string gameId);
    }
}
