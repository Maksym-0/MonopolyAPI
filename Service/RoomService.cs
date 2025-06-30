using Microsoft.AspNetCore.Mvc;
using Monopoly.Database;
using Monopoly.Models;
using System;
using System.Reflection.Metadata.Ecma335;

namespace Monopoly.Service
{
    public class RoomService
    {
        Random random = new Random();
        public async Task<string?> ValidJoinRoomAsync(string roomId, string? password, string accountName)
        {
            DBRoom dbRoom = new DBRoom();

            Room room = await dbRoom.ReadRoomAsync(roomId);
            if (room.MaxNumberOfPlayers <= room.NumberOfPlayers)
                return "Неможливо приєднатись. Кімнату переповнено";
            else if (room.InGame)
                return "Неможливо приєднатись. В кімнаті розпочато гру";
            else if (room.Password != null && room.Password != password)
                return "Неможливо приєднатись. Невірний пароль";
            else if (room.PlayerNames.Contains(accountName))
                return "Неможливо приєднатись. Гравець вже в даній кімнаті";
            else if (await CheckPlayerInRoom(accountName))
                return "Неможливо приєднатись. Гравець вже в іншій кімнаті";
            return null;
        }

        public async Task<List<Room>> GetAllRoomsAsync()
        {
            DBRoom dbRoom = new DBRoom();
            List<Room> rooms = await dbRoom.ReadRoomListAsync();
            for (int i = 0; i < rooms.Count; i++)
                rooms[i].Password = null;
            return rooms;
        }
        public async Task CreateRoomAsync(int maxNumberOfPlayers, string? password, string accountName)
        {
            DBRoom dbRoom = new DBRoom();
            List<string> players = new List<string>();
            players.Add(accountName);

            string roomId = await GenerateId(6);
            Room room;
            if (password != null)
                room = new Room(roomId, maxNumberOfPlayers, players, password);
            else
                room = new Room(roomId, maxNumberOfPlayers, players);

            await dbRoom.InsertRoomAsync(room);
        }
        public async Task<string> JoinRoomAsync(string roomId, string? password, string accountName)
        {
            DBRoom dbRoom = new DBRoom();
            Room room = await dbRoom.ReadRoomAsync(roomId);
            room.PlayerNames.Add(accountName);
            if (room.PlayerNames.Count >= room.MaxNumberOfPlayers)
            {
                room.InGame = true;
                await dbRoom.UpdateRoomAsync(room);
                DBCells dbCells = new DBCells();
                DBPlayerStatus dbPlayer = new DBPlayerStatus();

                List<Cell> cells = CreateCells(roomId);
                List<Player> players = CreatePlayers(roomId, room.PlayerNames);
                players[0].StartAction();

                await dbCells.InsertCellsAsync(cells);
                await dbPlayer.InsertPlayersAsync(players);

                return "Гравця додано до кімнати. Гру розпочато";
            }
            else
            {
                await dbRoom.UpdateRoomAsync(room);
                return "Гравця додано до кімнати";
            }
        }
        public async Task<string?> TryQuitRoomAsync(string roomId, string accountName)
        {
            DBRoom dbRoom = new DBRoom();
            Room room = await dbRoom.ReadRoomAsync(roomId);

            if (room.PlayerNames.Contains(accountName))
            {
                room.PlayerNames.Remove(accountName);

                if (room.PlayerNames.Count == 0)
                {
                    await dbRoom.UpdateRoomAsync(room);
                    return "Гравець покинув кімнату";
                }
                else
                {
                    await dbRoom.DeleteRoomAsync(roomId);
                    return "Гравець покинув кімнату. Порожню кімнату було видалено";
                }
            }
            return null;
        }

        private async Task<string> GenerateId(int length)
        {
            DBRoom dbRoom = new DBRoom();
            List<Room> rooms = await dbRoom.ReadRoomListAsync();
            string id = "";
            bool same = true;
            do
            {
                for (int i = 0; i < length; i++)
                {
                    id += random.Next(0, 10);
                }
                for (int i = 0; i < rooms.Count; i++)
                {
                    if (rooms[i].RoomId == id)
                        same = true;
                    else same = false;
                }
            } while (same);
            return id;
        }
        private async Task<bool> CheckPlayerInRoom(string name)
        {
            DBRoom dbRoom = new DBRoom();
            List<Room> rooms = await dbRoom.ReadRoomListAsync();

            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].PlayerNames.Contains(name))
                    return false;
            }
            return true;
        }
        private List<Cell> CreateCells(string gameId)
        {
            List<Cell> cells = new List<Cell>();
            Cell cell;
            for (int i = 0; i < Constants.CellNames.Count; i++)
            {
                if (Constants.SpecialCellNames.Contains(Constants.CellNames[i]))
                {
                    cell = new Cell(gameId, Constants.CellNames[i], i);
                }
                else
                {
                    cell = new Cell(gameId, Constants.CellNames[i], i, Constants.CellPrices[i], Constants.CellStartRents[i]);
                }
                cells.Add(cell);
            }
            return cells;
        }
        private List<Player> CreatePlayers(string gameId, List<string> names)
        {
            List<Player> players = new List<Player>();
            Player player;
            for (int i = 0; i < names.Count; i++)
            {
                player = new Player(gameId, names[i]);
                players.Add(player);
            }
            return players;
        }
    }
}
