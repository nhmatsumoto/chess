using System;
using System.Collections.Generic;
using System.Linq;
using Chess.Domain.Entities;
using Chess.Domain.Enums;
using Chess.Domain.ValueObjects;

namespace Chess.Domain.Logic;

public interface IMoveValidator
{
    List<Position> GetLegalMoves(ChessGame game, Position from);
    bool IsInCheck(ChessGame game, PieceColor color);
    void UpdateGameStatus(ChessGame game);
}

public static class MoveValidator
{
    // Keeping static methods for internal use or simple calls
    public static List<Position> GetLegalMoves(ChessGame game, Position from)
    {
        var piece = game.Board.GetPiece(from);
        if (piece == null || piece.Color != game.CurrentTurn) return new List<Position>();

        var pseudoMoves = GetPseudoLegalMoves(game, from);
        var legalMoves = new List<Position>();

        foreach (var to in pseudoMoves)
        {
            if (IsMoveLegal(game, from, to))
            {
                legalMoves.Add(to);
            }
        }

        // Add Castling
        if (piece.Type == PieceType.King && !piece.HasMoved && !IsInCheck(game, piece.Color))
        {
            if (CanCastle(game, piece.Color, true))
                legalMoves.Add(new Position(6, from.Rank));
            if (CanCastle(game, piece.Color, false))
                legalMoves.Add(new Position(2, from.Rank));
        }

        return legalMoves;
    }

    private static bool CanCastle(ChessGame game, PieceColor color, bool kingside)
    {
        int rank = color == PieceColor.White ? 0 : 7;
        int rookFile = kingside ? 7 : 0;
        var rook = game.Board.GetPiece(new Position(rookFile, rank));

        if (rook == null || rook.Type != PieceType.Rook || rook.HasMoved || rook.Color != color) return false;

        int[] path = kingside ? new[] { 5, 6 } : new[] { 1, 2, 3 };
        foreach (int f in path)
        {
            if (game.Board.GetPiece(new Position(f, rank)) != null) return false;
        }

        int[] checkPath = kingside ? new[] { 5, 6 } : new[] { 2, 3 };
        foreach (int f in checkPath)
        {
            if (IsSquareAttacked(game, new Position(f, rank), color == PieceColor.White ? PieceColor.Black : PieceColor.White)) return false;
        }

        return true;
    }

    public static bool IsInCheck(ChessGame game, PieceColor color)
    {
        var kingPos = FindKing(game.Board, color);
        return IsSquareAttacked(game, kingPos, color == PieceColor.White ? PieceColor.Black : PieceColor.White);
    }

    private static bool IsMoveLegal(ChessGame game, Position from, Position to)
    {
        var board = game.Board;
        var piece = board.GetPiece(from);
        var target = board.GetPiece(to);

        board.SetPiece(to, piece);
        board.SetPiece(from, null);

        bool inCheck = IsInCheck(game, piece!.Color);

        board.SetPiece(from, piece);
        board.SetPiece(to, target);

        return !inCheck;
    }

    public static List<Position> GetPseudoLegalMoves(ChessGame game, Position from)
    {
        var piece = game.Board.GetPiece(from);
        if (piece == null) return new List<Position>();

        var moves = new List<Position>();

        switch (piece.Type)
        {
            case PieceType.Pawn: GeneratePawnMoves(game, from, piece, moves); break;
            case PieceType.Rook: GenerateSlidingMoves(game, from, piece, new[] { (0, 1), (0, -1), (1, 0), (-1, 0) }, moves); break;
            case PieceType.Knight: GenerateSteppingMoves(game, from, piece, new[] { (1, 2), (1, -2), (-1, 2), (-1, -2), (2, 1), (2, -1), (-2, 1), (-2, -1) }, moves); break;
            case PieceType.Bishop: GenerateSlidingMoves(game, from, piece, new[] { (1, 1), (1, -1), (-1, 1), (-1, -1) }, moves); break;
            case PieceType.Queen: GenerateSlidingMoves(game, from, piece, new[] { (0, 1), (0, -1), (1, 0), (-1, 0), (1, 1), (1, -1), (-1, 1), (-1, -1) }, moves); break;
            case PieceType.King: GenerateSteppingMoves(game, from, piece, new[] { (0, 1), (0, -1), (1, 0), (-1, 0), (1, 1), (1, -1), (-1, 1), (-1, -1) }, moves); break;
        }

        return moves;
    }

    private static void GeneratePawnMoves(ChessGame game, Position from, ChessPiece piece, List<Position> moves)
    {
        int dir = piece.Color == PieceColor.White ? 1 : -1;
        var oneStep = new Position(from.File, from.Rank + dir);
        if (oneStep.IsValid() && game.Board.GetPiece(oneStep) == null)
        {
            moves.Add(oneStep);
            var twoStep = new Position(from.File, from.Rank + 2 * dir);
            if (!piece.HasMoved && twoStep.IsValid() && game.Board.GetPiece(twoStep) == null && from.Rank == (piece.Color == PieceColor.White ? 1 : 6))
                moves.Add(twoStep);
        }

        foreach (int side in new[] { -1, 1 })
        {
            var cap = new Position(from.File + side, from.Rank + dir);
            if (cap.IsValid())
            {
                var target = game.Board.GetPiece(cap);
                if (target != null && target.Color != piece.Color) moves.Add(cap);
                else if (game.EnPassantSquare != null && cap.File == game.EnPassantSquare.File && cap.Rank == game.EnPassantSquare.Rank) 
                    moves.Add(cap);
            }
        }
    }

    private static void GenerateSlidingMoves(ChessGame game, Position from, ChessPiece piece, (int dx, int dy)[] dirs, List<Position> moves)
    {
        foreach (var (dx, dy) in dirs)
        {
            int f = from.File + dx;
            int r = from.Rank + dy;
            while (true)
            {
                var pos = new Position(f, r);
                if (!pos.IsValid()) break;

                var target = game.Board.GetPiece(pos);
                if (target == null) moves.Add(pos);
                else
                {
                    if (target.Color != piece.Color) moves.Add(pos);
                    break;
                }
                f += dx;
                r += dy;
            }
        }
    }

    private static void GenerateSteppingMoves(ChessGame game, Position from, ChessPiece piece, (int dx, int dy)[] steps, List<Position> moves)
    {
        foreach (var (dx, dy) in steps)
        {
            var pos = new Position(from.File + dx, from.Rank + dy);
            if (pos.IsValid())
            {
                var target = game.Board.GetPiece(pos);
                if (target == null || target.Color != piece.Color) moves.Add(pos);
            }
        }
    }

    public static List<Position> GetAttackedSquares(ChessGame game, Position from)
    {
        var piece = game.Board.GetPiece(from);
        if (piece == null) return new List<Position>();

        var moves = new List<Position>();

        if (piece.Type == PieceType.Pawn)
        {
            int dir = piece.Color == PieceColor.White ? 1 : -1;
            foreach (int side in new[] { -1, 1 })
            {
                var cap = new Position(from.File + side, from.Rank + dir);
                if (cap.IsValid()) moves.Add(cap);
            }
        }
        else
        {
            // For other pieces, attacks are same as pseudo-legal moves (ignoring check)
            moves = GetPseudoLegalMoves(game, from);
        }

        return moves;
    }

    public static bool IsSquareAttacked(ChessGame game, Position pos, PieceColor byColor)
    {
        for (int f = 0; f < 8; f++)
            for (int r = 0; r < 8; r++)
            {
                var atPos = new Position(f, r);
                var p = game.Board.GetPiece(atPos);
                if (p != null && p.Color == byColor)
                {
                    if (GetAttackedSquares(game, atPos).Any(m => m.File == pos.File && m.Rank == pos.Rank)) return true;
                }
            }
        return false;
    }

    private static Position FindKing(ChessBoard board, PieceColor color)
    {
        for (int f = 0; f < 8; f++)
            for (int r = 0; r < 8; r++)
            {
                var pos = new Position(f, r);
                var p = board.GetPiece(pos);
                if (p != null && p.Color == color && p.Type == PieceType.King) return pos;
            }
        throw new Exception("King not found!");
    }

    public static void UpdateGameStatus(ChessGame game)
    {
        bool hasLegalMoves = false;
        for (int f = 0; f < 8; f++)
            for (int r = 0; r < 8; r++)
            {
                var pos = new Position(f, r);
                var p = game.Board.GetPiece(pos);
                if (p != null && p.Color == game.CurrentTurn)
                {
                    if (GetLegalMoves(game, pos).Any()) { hasLegalMoves = true; break; }
                }
            }

        if (!hasLegalMoves)
        {
            if (IsInCheck(game, game.CurrentTurn)) game.SetStatus(GameStatus.Checkmate);
            else game.SetStatus(GameStatus.Stalemate);
        }
    }
}

public class ChessMoveValidator : IMoveValidator
{
    public List<Position> GetLegalMoves(ChessGame game, Position from) => MoveValidator.GetLegalMoves(game, from);
    public bool IsInCheck(ChessGame game, PieceColor color) => MoveValidator.IsInCheck(game, color);
    public void UpdateGameStatus(ChessGame game) => MoveValidator.UpdateGameStatus(game);
}
