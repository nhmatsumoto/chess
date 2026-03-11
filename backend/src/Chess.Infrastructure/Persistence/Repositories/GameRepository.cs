using System;
using System.Collections.Generic;
using System.Linq;
using Chess.Application.Common.Interfaces;
using Chess.Domain.Entities;
using Chess.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Chess.Infrastructure.Persistence.Repositories;

public class GameRepository : IGameRepository
{
    private readonly ChessDbContext _context;

    public GameRepository(ChessDbContext context)
    {
        _context = context;
    }

    public async Task<ChessGame?> GetByIdAsync(Guid id)
    {
        var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == id);
        if (game == null) return null;

        var pieces = await _context.Pieces.Where(p => p.GameId == id).ToListAsync();
        foreach (var p in pieces)
        {
            game.Board.SetPiece(new Position(p.File, p.Rank), p);
        }
        
        // Note: For a truly robust system, we'd persist more state (last move, etc.)
        return game;
    }

    public async Task AddAsync(ChessGame game)
    {
        _context.Games.Add(game);
        await PersistBoard(game);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ChessGame game)
    {
        _context.Games.Update(game);
        
        // Refresh pieces
        var existingPieces = await _context.Pieces.Where(p => p.GameId == game.Id).ToListAsync();
        _context.Pieces.RemoveRange(existingPieces);
        
        await PersistBoard(game);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ChessGame>> GetAllAsync()
    {
        return await _context.Games.ToListAsync();
    }

    private async Task PersistBoard(ChessGame game)
    {
        for (int f = 0; f < 8; f++)
        {
            for (int r = 0; r < 8; r++)
            {
                var p = game.Board.GetPiece(new Position(f, r));
                if (p != null)
                {
                    p.File = f;
                    p.Rank = r;
                    p.GameId = game.Id;
                    _context.Pieces.Add(p);
                }
            }
        }
    }
}
