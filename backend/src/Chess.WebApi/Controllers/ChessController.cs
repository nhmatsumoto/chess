using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chess.Application.Common.Interfaces;

namespace Chess.WebApi.Controllers
{
    [ApiController]
    [Route("api/chess")]
    public class ChessController : ControllerBase
    {
        private readonly IGameService _gameService;

        public ChessController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost("rooms")]
        public async Task<IActionResult> CreateRoom()
        {
            var roomId = await _gameService.CreateGameAsync();
            return Ok(new { roomId });
        }

        [HttpGet("rooms/{roomId}")]
        public async Task<IActionResult> GetGame(string roomId)
        {
            var game = await _gameService.GetGameAsync(roomId);
            if (game == null) return NotFound("Room not found");
            return Ok(game);
        }

        [HttpGet("rooms/{roomId}/moves")]
        public async Task<IActionResult> GetLegalMoves(string roomId, [FromQuery] string pos)
        {
            try 
            {
                var moves = await _gameService.GetLegalMovesAsync(roomId, pos);
                return Ok(moves);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("rooms/{roomId}/move")]
        public async Task<IActionResult> MakeMove(string roomId, [FromBody] MoveRequest request)
        {
            if (await _gameService.MakeMoveAsync(roomId, request.From, request.To))
            {
                return Ok(new { success = true });
            }
            return BadRequest("Invalid move");
        }
    }

    public class MoveRequest
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
    }
}
