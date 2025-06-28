using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Monopoly.Models;
using Monopoly.Database;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;

namespace Monopoly.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Authorize]
    public class RoomController : ControllerBase
    {
        Random random = new Random();
        private readonly ILogger<GameController> logger;
        public RoomController(ILogger<GameController> logger)
        {
            this.logger = logger;
        }
        [HttpPost]
        [ActionName("CreateRoom")]
        public async Task<IActionResult> CreateRoom(int maxNumberOfPlayers, string? password)
        {
            if (maxNumberOfPlayers < 2 || maxNumberOfPlayers > 4)
                return BadRequest("Максимальна кількість гравців обмежена від 2 до 4 осіб");

            DBRoom dbRoom = new DBRoom();
            List<string> players = new List<string>();
            players.Add(User.Identity.Name);

            string roomId = GenerateId(6);
            Room room;
            if (password != null)
                room = new Room(roomId, maxNumberOfPlayers, players, password);
            else
                room = new Room(roomId, maxNumberOfPlayers, players);

            await dbRoom.InsertRoomAsync(room);

            return Created();
        }
        [HttpGet]
        [ActionName("GetRooms")]
        public async Task<IActionResult> GetRooms()
        {
            DBRoom dbRoom = new DBRoom();
            List<Room> rooms = await dbRoom.ReadRoomListAsync();
            return Ok(rooms);
        }
        [HttpPost]
        [ActionName("JoinRoom")]
        public async Task<IActionResult> JoinRoom(string roomId, string? password)
        {
            DBRoom dbRoom = new DBRoom();

            Room room = await dbRoom.ReadRoomAsync(roomId);
            if (room.MaxNumberOfPlayers <= room.NumberOfPlayers)
                return Conflict("Неможливо приєднатись. Кімнату переповнено");
            else if (room.InGame)
                return Conflict("Неможливо приєднатись. В кімнаті розпочато гру");
            else if (room.Password != null && room.Password != password)
                return Unauthorized("Неможливо приєднатись. Невірний пароль");
            else if (room.PlayerNames.Contains(User.Identity.Name))
                return Conflict("Неможливо приєднатись. Гравець вже в даній кімнаті");
            else if (await CheckPlayerInRoom(User.Identity.Name))
                return Conflict("Неможливо приєднатись. Гравець вже в іншій кімнаті");
            else
            {
                room.PlayerNames.Add(User.Identity.Name);
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
                    
                    return Ok("Гравця додано до кімнати. Гру розпочато");
                }
                else
                {
                    await dbRoom.UpdateRoomAsync(room);
                    return Ok("Гравця додано до кімнати");
                }
            }
        }
        [HttpPost]
        [ActionName("QuitRoom")]
        public async Task<IActionResult> QuitRoom(string roomId)
        {
            DBRoom dbRoom = new DBRoom();

            Room room = await dbRoom.ReadRoomAsync(roomId);
            if (room.PlayerNames.Contains(User.Identity.Name))
            {
                room.PlayerNames.Remove(User.Identity.Name);
                
                if(room.PlayerNames.Count == 0)
                {
                    await dbRoom.UpdateRoomAsync(room);
                    return Ok("Гравець покинув кімнату");
                }
                else
                {
                    await dbRoom.DeleteRoomAsync(roomId);
                    return Ok("Гравець покинув кімнату. Порожню кімнату було видалено");
                }
            }
            else
                return BadRequest("Неможливо покинути кімнату. Гравець відсутній в ній");
        }
        
        [NonAction]
        private string GenerateId(int length)
        {
            string id = "";
            for(int i = 0; i < length; i++)
            {
                id += random.Next(0, 10);
            }
            return id;
        }
        [NonAction]
        private async Task<bool> CheckPlayerInRoom(string name)
        {
            DBRoom dbRoom = new DBRoom();
            List<Room> rooms = await dbRoom.ReadRoomListAsync();
            
            for(int i = 0; i <  rooms.Count; i++)
            {
                if (rooms[i].PlayerNames.Contains(name))
                    return false;
            }
            return true;
        }
        [NonAction]
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
        [NonAction]
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
