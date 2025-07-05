using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monopoly.Interfaces.IServices;
using Monopoly.Models.ApiResponse;
using Monopoly.Models.APIResponse;
using Monopoly.Models.RoomModels;
using System.Net;
using System.Security.Claims;

namespace Monopoly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet("rooms")]
        public async Task<IActionResult> GetRooms()
        {
            try
            {
                List<RoomResponse> rooms = await _roomService.GetAllRoomsAsync();
                ApiResponse<List<RoomResponse>> response = new ApiResponse<List<RoomResponse>>()
                {
                    Success = true,
                    Message = "Отримано список кімнат",
                    Data = rooms
                };
                return Ok(response); 
            }
            catch(Exception ex)
            {
                return CatchBadRequest(ex);
            }
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateRoom(int maxNumberOfPlayers, string? password)
        {
            try
            {
                RoomResponse room = await _roomService.CreateRoomAsync(maxNumberOfPlayers, password, User.FindFirst(ClaimTypes.NameIdentifier).Value, User.Identity.Name);
                ApiResponse<RoomResponse> response = new ApiResponse<RoomResponse>()
                {
                    Success = true,
                    Message = "Кімнату успішно створено",
                    Data = room
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return CatchBadRequest(ex);
            }
        }
        [HttpPut("{roomId}/join")]
        public async Task<IActionResult> JoinRoom(string roomId, string? password)
        {
            try
            {
                RoomResponse room = await _roomService.JoinRoomAsync(roomId, password, User.FindFirst(ClaimTypes.NameIdentifier).Value, User.Identity.Name);
                string msg = "Ви зайшли до кімнати";
                if (room.InGame)
                    msg = "Ви зайшли до кімнати. Гру розпочато";
                ApiResponse<RoomResponse> response = new ApiResponse<RoomResponse>()
                {
                    Success = true,
                    Message = msg,
                    Data = room
                };
                return Ok(response);
            }
            catch(Exception ex)
            {
                return CatchBadRequest(ex);
            }
        }
        [HttpPut("quit")]
        public async Task<IActionResult> QuitRoom()
        {
            try
            {
                string msg = await _roomService.QuitRoomAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                ApiResponse<RoomResponse> response = new ApiResponse<RoomResponse>()
                {
                    Success = true,
                    Message = msg,
                    Data = null
                };
                return Ok(response);
            }
            catch(Exception ex)
            {
                return CatchBadRequest(ex);
            }
        }

        private IActionResult CatchBadRequest(Exception ex)
        {
            ApiResponse<List<RoomResponse>> response = new ApiResponse<List<RoomResponse>>()
            {
                Success = false,
                Message = ex.Message,
                Data = null
            };
            return BadRequest(response);
        }
    }
}
