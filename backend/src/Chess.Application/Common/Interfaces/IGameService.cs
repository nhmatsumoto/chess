using Chess.Application.Common.DTOs;

namespace Chess.Application.Common.Interfaces;

public interface IGameService
{
    Task<string> CreateGameAsync();
    Task<GameDto?> GetGameAsync(string id);
    Task<bool> MakeMoveAsync(string id, string from, string to);
    Task<IEnumerable<string>> GetLegalMovesAsync(string id, string pos);
}
