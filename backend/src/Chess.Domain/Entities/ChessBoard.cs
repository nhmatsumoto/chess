using System;
using Chess.Domain.Enums;
using Chess.Domain.ValueObjects;

namespace Chess.Domain.Entities;

public class ChessBoard
{
    private readonly ChessPiece?[,] _pieces = new ChessPiece?[8, 8];

    public ChessPiece? GetPiece(Position pos) => _pieces[pos.File, pos.Rank];

    public void SetPiece(Position pos, ChessPiece? piece) => _pieces[pos.File, pos.Rank] = piece;

    public void InitializeStandard()
    {
        // Initialize pawns
        for (int i = 0; i < 8; i++)
        {
            SetPiece(new Position(i, 1), new ChessPiece(PieceType.Pawn, PieceColor.White));
            SetPiece(new Position(i, 6), new ChessPiece(PieceType.Pawn, PieceColor.Black));
        }

        // Initialize other pieces
        PieceType[] backline = { PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen, PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook };
        for (int i = 0; i < 8; i++)
        {
            SetPiece(new Position(i, 0), new ChessPiece(backline[i], PieceColor.White));
            SetPiece(new Position(i, 7), new ChessPiece(backline[i], PieceColor.Black));
        }
    }
    
    // For EF Core mapping later
    public ChessPiece?[,] GetRawPieces() => _pieces;
}
