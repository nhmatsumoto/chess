using Microsoft.AspNetCore.Mvc;
using ChessBackend.Models;
using ChessBackend.Logic;
using ChessBackend.Services;
using System.Collections.Generic;
using System.Linq;

namespace ChessBackend.Controllers
{
    [ApiController]
    [Route("api/chess")]
    public class ChessController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public ChessController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpPost("rooms")]
        public IActionResult CreateRoom()
        {
            var room = _roomService.CreateRoom();
            return Ok(new { roomId = room.Id });
        }

        [HttpGet("rooms/{roomId}")]
        public IActionResult GetGame(string roomId)
        {
            var room = _roomService.GetRoom(roomId);
            if (room == null) return NotFound("Room not found");

            var game = room.Game;
            return Ok(new
            {
                board = game.Board.Squares.SelectMany((row, r) => 
                    row.Select((p, f) => new { 
                        type = p?.Type.ToString(), 
                        color = p?.Color.ToString(), 
                        pos = new Position(f, r).ToString() 
                    }).Where(x => x.type != null)),
                turn = game.CurrentTurn.ToString(),
                history = game.MoveHistory,
                status = game.Status.ToString()
            });
        }

        [HttpGet("rooms/{roomId}/moves")]
        public IActionResult GetLegalMoves(string roomId, [FromQuery] string pos)
        {
            var room = _roomService.GetRoom(roomId);
            if (room == null) return NotFound("Room not found");

            var position = Position.FromString(pos);
            var moves = MoveValidator.GetLegalMoves(room.Game, position);
            return Ok(moves.Select(m => m.ToString()));
        }

        [HttpPost("rooms/{roomId}/move")]
        public IActionResult MakeMove(string roomId, [FromBody] MoveRequest request)
        {
            var room = _roomService.GetRoom(roomId);
            if (room == null) return NotFound("Room not found");

            var game = room.Game;
            var from = Position.FromString(request.From);
            var to = Position.FromString(request.To);

            if (game.MakeMove(from, to))
            {
                return Ok(new { success = true, status = game.Status.ToString() });
            }
            return BadRequest("Invalid move");
        }
    }

    public class MoveRequest
    {
        public string From { get; set; }
        public string To { get; set; }
    }
}
