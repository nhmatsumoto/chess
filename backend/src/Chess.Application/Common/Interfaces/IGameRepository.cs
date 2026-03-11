using Chess.Domain.Entities;

namespace Chess.Application.Common.Interfaces;

public interface IGameRepository
{
    Task<ChessGame?> GetByIdAsync(Guid id);
    Task AddAsync(ChessGame game);
    Task UpdateAsync(ChessGame game);
    Task<IEnumerable<ChessGame>> GetAllAsync();
}
