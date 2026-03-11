using System;
using Chess.Domain.Enums;
using Chess.Domain.ValueObjects;

namespace Chess.Domain.Entities;

public class ChessPiece
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public PieceType Type { get; set; }
    public PieceColor Color { get; set; }
    public bool HasMoved { get; set; }
    public int File { get; set; }
    public int Rank { get; set; }
    public Guid GameId { get; set; }

    public ChessPiece(PieceType type, PieceColor color)
    {
        Type = type;
        Color = color;
        HasMoved = false;
    }
}
