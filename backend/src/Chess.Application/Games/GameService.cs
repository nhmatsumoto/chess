using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chess.Application.Common.DTOs;
using Chess.Application.Common.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.Logic;
using Chess.Domain.ValueObjects;

namespace Chess.Application.Games;

public class GameService : IGameService
{
    private readonly IGameRepository _repository;
    private readonly IMoveValidator _validator;

    public GameService(IGameRepository repository, IMoveValidator validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<string> CreateGameAsync()
    {
        var game = new ChessGame();
        await _repository.AddAsync(game);
        return game.Id.ToString();
    }

    public async Task<GameDto?> GetGameAsync(string id)
    {
        if (!Guid.TryParse(id, out var guid)) return null;
        var game = await _repository.GetByIdAsync(guid);
        if (game == null) return null;

        var board = new List<PieceDto>();
        for (int f = 0; f < 8; f++)
        {
            for (int r = 0; r < 8; r++)
            {
                var pos = new Position(f, r);
                var p = game.Board.GetPiece(pos);
                if (p != null)
                {
                    board.Add(new PieceDto(p.Type.ToString(), p.Color.ToString(), pos.ToString()));
                }
            }
        }

        return new GameDto(
            game.Id.ToString(),
            board,
            game.CurrentTurn.ToString(),
            game.MoveHistory,
            game.Status.ToString()
        );
    }

    public async Task<bool> MakeMoveAsync(string id, string from, string to)
    {
        if (!Guid.TryParse(id, out var guid)) return false;
        var game = await _repository.GetByIdAsync(guid);
        if (game == null) return false;

        var fromPos = Position.FromString(from);
        var toPos = Position.FromString(to);

        if (game.MakeMove(fromPos, toPos, _validator))
        {
            await _repository.UpdateAsync(game);
            return true;
        }

        return false;
    }

    public async Task<IEnumerable<string>> GetLegalMovesAsync(string id, string pos)
    {
        if (!Guid.TryParse(id, out var guid)) return Enumerable.Empty<string>();
        var game = await _repository.GetByIdAsync(guid);
        if (game == null) return Enumerable.Empty<string>();

        var fromPos = Position.FromString(pos);
        var moves = _validator.GetLegalMoves(game, fromPos);
        return moves.Select(m => m.ToString());
    }
}
