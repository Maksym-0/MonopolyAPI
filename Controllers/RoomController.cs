using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Monopoly.Models;
using Monopoly.Database;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Monopoly.Service;

namespace Monopoly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoomController : ControllerBase
    {
        private readonly ILogger<RoomController> logger;
        private RoomService roomService = new RoomService();
        public RoomController(ILogger<RoomController> logger)
        {
            this.logger = logger;
        }
        [HttpGet("rooms")]
        public async Task<IActionResult> GetRooms()
        {
            List<Room> rooms = await roomService.GetAllRoomsAsync();
            return Ok(rooms);
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateRoom(int maxNumberOfPlayers, string? password)
        {
            if (maxNumberOfPlayers < 2 || maxNumberOfPlayers > 4)
                return BadRequest("Максимальна кількість гравців обмежена від 2 до 4 осіб");

            await roomService.CreateRoomAsync(maxNumberOfPlayers, password, User.Identity.Name);
            return Created();
        }
        [HttpPut("{roomId}/join")]
        public async Task<IActionResult> JoinRoom(string roomId, string? password)
        {
            string? check = await roomService.ValidJoinRoomAsync(roomId, password, User.Identity.Name);
            if (check != null)
                return BadRequest(check);
            else
                return Ok(await roomService.JoinRoomAsync(roomId, password, User.Identity.Name));
        }
        [HttpPut("{roomId}/quit")]
        public async Task<IActionResult> QuitRoom(string roomId)
        {
            string? check = await roomService.TryQuitRoomAsync(roomId, User.Identity.Name);
            if (check != null)
                return Ok(check);
            else
                return BadRequest("Неможливо покинути кімнату. Гравець відсутній в ній");
        }
    }
}
