using ChessBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChessBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChessController : ControllerBase
    {
        private static ChessGame? _game;

        [HttpGet]
        public IActionResult GetGame()
        {
            if (_game == null) _game = new ChessGame();
            
            return Ok(new
            {
                board = SerializeBoard(_game.Board),
                turn = _game.CurrentTurn.ToString(),
                history = _game.MoveHistory
            });
        }

        [HttpPost("move")]
        public IActionResult MakeMove([FromBody] MoveRequest request)
        {
            if (_game == null) _game = new ChessGame();

            var from = Position.FromString(request.From);
            var to = Position.FromString(request.To);

            if (_game.ExecuteMove(from, to))
            {
                return Ok(new { success = true, board = SerializeBoard(_game.Board), turn = _game.CurrentTurn.ToString() });
            }

            return BadRequest("Invalid move");
        }

        private List<object> SerializeBoard(ChessBoard board)
        {
            var pieces = new List<object>();
            for (int f = 0; f < 8; f++)
            {
                for (int r = 0; r < 8; r++)
                {
                    var piece = board.GetPiece(new Position(f, r));
                    if (piece != null)
                    {
                        pieces.Add(new
                        {
                            type = piece.Type.ToString(),
                            color = piece.Color.ToString(),
                            pos = new Position(f, r).ToString()
                        });
                    }
                }
            }
            return pieces;
        }
    }

    public class MoveRequest
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
    }
}
