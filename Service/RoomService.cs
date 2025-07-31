using Monopoly.Database;
using Monopoly.Interfaces.IDatabases;
using Monopoly.Interfaces.IServices;
using Monopoly.Models.APIResponse;
using Monopoly.Models.GameModels;
using Monopoly.Models.RoomModels;

namespace Monopoly.Service
{
    public class RoomService : IRoomService
    {
        private Random random = new Random();
        private readonly IRoomRepository dbRoom;
        private readonly ICellRepository dbCells;
        private readonly IPlayerInRoomRepository dbPlayerInRoom;
        private readonly IPlayerRepository dbPlayer;

        public RoomService(IRoomRepository roomRepository, ICellRepository cellRepository, 
            IPlayerInRoomRepository playerInRoomRepository, IPlayerRepository playerRepository) 
        {
            dbRoom = roomRepository;
            dbCells = cellRepository;
            dbPlayerInRoom = playerInRoomRepository;
            dbPlayer = playerRepository;
        }

        public async Task<List<RoomResponse>> GetAllRoomsAsync()
        {
            List<Room> rooms = await dbRoom.ReadRoomListAsync();
            List<RoomResponse> roomResponces = new List<RoomResponse>();

            if (rooms.Count == 0)
                return new List<RoomResponse>();
            for (int i = 0; i < rooms.Count; i++)
                roomResponces.Add(new RoomResponse(rooms[i], await dbPlayerInRoom.ReadPlayerInRoomListAsync(rooms[i].RoomId)));
            return roomResponces;
        }
        public async Task<RoomResponse> CreateRoomAsync(int maxNumberOfPlayers, string? password, string accountId, string accountName)
        {
            string? isValid = await ValidCreateRoomAsync(maxNumberOfPlayers, accountId);
            if (isValid != null)
                throw new Exception(isValid);

            List<PlayerInRoom> basicPlayers = new List<PlayerInRoom>();

            string roomId = await GenerateId(6);
            basicPlayers.Add(new PlayerInRoom(roomId, accountId, accountName));
            
            Room room;
            if (password != null)
                room = new Room(roomId, maxNumberOfPlayers, basicPlayers.Count, password);
            else
                room = new Room(roomId, maxNumberOfPlayers, basicPlayers.Count);
            
            await dbRoom.InsertRoomAsync(room);
            await dbPlayerInRoom.InsertPlayerInRoomListAsync(basicPlayers);

            return new RoomResponse(room, basicPlayers);
        }
        public async Task<RoomResponse> JoinRoomAsync(string roomId, string? password, string accountId, string accountName)
        {
            string? isValid = await ValidJoinRoomAsync(roomId, password, accountId);
            if (isValid != null)
                throw new Exception(isValid);
            
            Room room = await dbRoom.ReadRoomAsync(roomId);
            
            await dbPlayerInRoom.InsertPlayerInRoomAsync(new PlayerInRoom(roomId, accountId, accountName));
            room.CountOfPlayers += 1;
            
            if (room.CountOfPlayers >= room.MaxNumberOfPlayers)
                await StartGameInRoom(room);
            else
                await dbRoom.UpdateRoomAsync(room);
            return new RoomResponse(room, await dbPlayerInRoom.ReadPlayerInRoomListAsync(roomId));
        }
        public async Task<string> QuitRoomAsync(string accountId)
        {
            string? isValid = await ValidQuitRoomAsync(accountId);
            if (isValid != null)
                throw new Exception(isValid);

            PlayerInRoom playerToRemove = await dbPlayerInRoom.ReadPlayerInRoomAsync(accountId);
            Room room = await dbRoom.ReadRoomAsync(playerToRemove.RoomId);

            await dbPlayerInRoom.DeletePlayerInRoomAsync(accountId);
            room.CountOfPlayers -= 1;
                
            if (room.CountOfPlayers > 0)
            {
                await dbRoom.UpdateRoomAsync(room);
                return "Гравець покинув кімнату";
            }
            else
            {
                await DeleteRoomAndPlayers(playerToRemove.RoomId);
                return "Гравець покинув кімнату. Порожню кімнату було видалено";
            }
        }

        private async Task<string?> ValidCreateRoomAsync(int maxNumberOfPlayers, string accountId)      
        {
            if (await dbPlayerInRoom.SearchPlayerInRoomWithIdAsync(accountId))
                return "Неможливо створити кімнату. Гравець вже перебуває в кімнаті";
            else if (maxNumberOfPlayers < 2 || maxNumberOfPlayers > 4)
                return "Максимальна кількість гравців обмежена від 2 до 4 осіб";
            return null;
        }
        private async Task<string?> ValidJoinRoomAsync(string roomId, string? password, string playerId)
        {
            Room room = await dbRoom.ReadRoomAsync(roomId);
            if (room.MaxNumberOfPlayers <= room.CountOfPlayers)
                return "Неможливо приєднатись. Кімнату переповнено";
            else if (room.InGame)
                return "Неможливо приєднатись. В кімнаті розпочато гру";
            else if (room.Password != null && room.Password != password)
                return "Неможливо приєднатись. Невірний пароль";
            else if (await CheckPlayerInAnyRoomAsync(playerId))
                return "Неможливо приєднатись. Гравець вже в кімнаті";
            return null;
        }
        private async Task<string?> ValidQuitRoomAsync(string accountId)
        {
            if (!await dbPlayerInRoom.SearchPlayerInRoomWithIdAsync(accountId))
                return "Неможливо покинути кімнату. Гравець не перебуває в жодній кімнаті";
            return null;
        }

        private async Task<string> GenerateId(int length)
        {
            string id = "";
            do
            {
                id = "";
                for (int i = 0; i < length; i++)
                {
                    id += random.Next(0, 10);
                }
            } while (await dbRoom.SearchRoomWithIdAsync(id));
            return id;
        }
        private async Task<bool> CheckPlayerInAnyRoomAsync(string playerId)
        {
            bool data = await dbPlayerInRoom.SearchPlayerInRoomWithIdAsync(playerId);
            return data;
        }
        private async Task StartGameInRoom(Room room)
        {
            room.InGame = true;
            await dbRoom.UpdateRoomAsync(room);

            List<Cell> cells = CreateCells(room.RoomId);
            List<Player> players = CreatePlayers(await dbPlayerInRoom.ReadPlayerInRoomListAsync(room.RoomId));
            
            players[0].StartAction();

            await dbCells.InsertCellsAsync(cells);
            await dbPlayer.InsertPlayersAsync(players);
        }
        private async Task DeleteRoomAndPlayers(string roomId)
        {
            var deletePlayer = dbRoom.DeleteRoomAsync(roomId);
            var deleteRoom = dbPlayerInRoom.DeleteAllPlayersInRoomAsync(roomId);

            await Task.WhenAll(deletePlayer, deleteRoom);
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
        private List<Player> CreatePlayers(List<PlayerInRoom> playersInfo)
        {
            List<Player> players = new List<Player>();
            Player player;
            for (int i = 0; i < playersInfo.Count; i++)
            {
                player = new Player(playersInfo[i]);
                players.Add(player);
            }
            return players;
        }
    }
}
