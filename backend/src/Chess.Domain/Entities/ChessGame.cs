using System;
using System.Collections.Generic;
using System.Linq;
using Chess.Domain.Enums;
using Chess.Domain.ValueObjects;

namespace Chess.Domain.Entities;

public class ChessGame
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ChessBoard Board { get; } = new();
    public PieceColor CurrentTurn { get; private set; } = PieceColor.White;
    public List<string> MoveHistory { get; } = new();
    public Position? EnPassantSquare { get; private set; }
    public GameStatus Status { get; private set; } = GameStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ChessGame()
    {
        Board.InitializeStandard();
    }

    public void SetStatus(GameStatus status) => Status = status;
    public void SetEnPassantSquare(Position? pos) => EnPassantSquare = pos;
    public void SwitchTurn() => CurrentTurn = CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;

    public bool MakeMove(Position from, Position to, Logic.IMoveValidator validator)
    {
        if (Status != GameStatus.Active) return false;

        var piece = Board.GetPiece(from);
        if (piece == null || piece.Color != CurrentTurn) return false;

        var legalMoves = validator.GetLegalMoves(this, from);
        if (!legalMoves.Any(m => m.File == to.File && m.Rank == to.Rank)) return false;

        // Special Move: En Passant Capture
        if (piece.Type == PieceType.Pawn && EnPassantSquare != null && to.File == EnPassantSquare.File && to.Rank == EnPassantSquare.Rank)
        {
            Board.SetPiece(new Position(to.File, from.Rank), null);
        }

        // Special Move: Castling
        if (piece.Type == PieceType.King && Math.Abs(to.File - from.File) == 2)
        {
            bool kingside = to.File == 6;
            int rookFromIdx = kingside ? 7 : 0;
            int rookToIdx = kingside ? 5 : 3;
            var rook = Board.GetPiece(new Position(rookFromIdx, from.Rank));
            Board.SetPiece(new Position(rookToIdx, from.Rank), rook);
            Board.SetPiece(new Position(rookFromIdx, from.Rank), null);
            if (rook != null) rook.HasMoved = true;
        }

        // Execute Move
        Board.SetPiece(to, piece);
        Board.SetPiece(from, null);

        // Pawn Promotion
        if (piece.Type == PieceType.Pawn && (to.Rank == 0 || to.Rank == 7))
        {
            Board.SetPiece(to, new ChessPiece(PieceType.Queen, piece.Color) { HasMoved = true });
        }

        piece.HasMoved = true;

        // Update En Passant Square
        EnPassantSquare = null;
        if (piece.Type == PieceType.Pawn && Math.Abs(to.Rank - from.Rank) == 2)
        {
            EnPassantSquare = new Position(from.File, (from.Rank + to.Rank) / 2);
        }

        MoveHistory.Add($"{from} to {to}");
        SwitchTurn();
        
        validator.UpdateGameStatus(this);

        return true;
    }
}

public enum GameStatus { Active, Checkmate, Stalemate, Draw }
