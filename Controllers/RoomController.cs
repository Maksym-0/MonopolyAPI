using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monopoly.Interfaces.IServices;
using Monopoly.Models.ApiResponse;
using Monopoly.Models.APIResponse;
using Monopoly.Models.Request;
using System.Security.Claims;

namespace Monopoly.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    [Authorize]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRooms()
        {
            try
            {
                List<RoomDto> rooms = await _roomService.GetAllRoomsAsync();
                ApiResponse<List<RoomDto>> response = new ApiResponse<List<RoomDto>>()
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
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest dto)
        {
            try
            {
                RoomDto room = await _roomService.CreateRoomAsync(dto.MaxNumberOfPlayers, dto.Password, User.FindFirst(ClaimTypes.NameIdentifier).Value, User.Identity.Name);
                ApiResponse<RoomDto> response = new ApiResponse<RoomDto>()
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
        public async Task<IActionResult> JoinRoom([FromBody] JoinRoomRequest dto)
        {
            try
            {
                RoomDto room = await _roomService.JoinRoomAsync(dto.RoomId, dto.Password, User.FindFirst(ClaimTypes.NameIdentifier).Value, User.Identity.Name);
                string msg = "Ви зайшли до кімнати";
                if (room.InGame)
                    msg = "Ви зайшли до кімнати. Гру розпочато";
                ApiResponse<RoomDto> response = new ApiResponse<RoomDto>()
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
                ApiResponse<RoomDto> response = new ApiResponse<RoomDto>()
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
            ApiResponse<List<RoomDto>> response = new ApiResponse<List<RoomDto>>()
            {
                Success = false,
                Message = ex.Message,
                Data = null
            };
            return BadRequest(response);
        }
    }
}
