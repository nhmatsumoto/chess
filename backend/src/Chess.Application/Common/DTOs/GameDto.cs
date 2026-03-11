namespace Chess.Application.Common.DTOs;

public record GameDto(
    string Id,
    IEnumerable<PieceDto> Board,
    string Turn,
    IEnumerable<string> History,
    string Status
);

public record PieceDto(string Type, string Color, string Pos);
