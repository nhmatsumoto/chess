using System;
using System.Collections.Generic;
using System.Linq;
using ChessBackend.Logic;

namespace ChessBackend.Models
{
    public enum GameStatus { Active, Checkmate, Stalemate, Draw }

    public class ChessGame
    {
        public ChessBoard Board { get; }
        public PieceColor CurrentTurn { get; private set; } = PieceColor.White;
        public List<string> MoveHistory { get; } = new List<string>();
        public Position? EnPassantSquare { get; private set; }
        public GameStatus Status { get; private set; } = GameStatus.Active;

        public ChessGame()
        {
            Board = new ChessBoard();
            Board.InitializeStandard();
        }

        public bool MakeMove(Position from, Position to)
        {
            if (Status != GameStatus.Active) return false;

            var piece = Board.GetPiece(from);
            if (piece == null || piece.Color != CurrentTurn) return false;

            var legalMoves = MoveValidator.GetLegalMoves(this, from);
            if (!legalMoves.Any(m => m.File == to.File && m.Rank == to.Rank)) return false;

            // Handle En Passant Capture
            if (piece.Type == PieceType.Pawn && EnPassantSquare != null && to.File == EnPassantSquare.File && to.Rank == EnPassantSquare.Rank)
            {
                Board.SetPiece(new Position(to.File, from.Rank), null);
            }

            // Handle Castling Rook Move
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

            // Handle Pawn Promotion (Auto-promote to Queen for simplicity)
            if (piece.Type == PieceType.Pawn && (to.Rank == 0 || to.Rank == 7))
            {
                Board.SetPiece(to, new ChessPiece(PieceType.Queen, piece.Color) { HasMoved = true });
            }

            // Update Piece State
            piece.HasMoved = true;

            // Update En Passant Square
            EnPassantSquare = null;
            if (piece.Type == PieceType.Pawn && Math.Abs(to.Rank - from.Rank) == 2)
            {
                EnPassantSquare = new Position(from.File, (from.Rank + to.Rank) / 2);
            }

            // Record History
            MoveHistory.Add($"{from} to {to}");

            // Switch Turn
            CurrentTurn = CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;

            // Check for Game End
            UpdateGameStatus();

            return true;
        }

        private void UpdateGameStatus()
        {
            bool hasLegalMoves = false;
            for (int f = 0; f < 8; f++)
            {
                for (int r = 0; r < 8; r++)
                {
                    var pos = new Position(f, r);
                    var p = Board.GetPiece(pos);
                    if (p != null && p.Color == CurrentTurn)
                    {
                        if (MoveValidator.GetLegalMoves(this, pos).Any())
                        {
                            hasLegalMoves = true;
                            break;
                        }
                    }
                }
                if (hasLegalMoves) break;
            }

            if (!hasLegalMoves)
            {
                if (MoveValidator.IsInCheck(this, CurrentTurn))
                    Status = GameStatus.Checkmate;
                else
                    Status = GameStatus.Stalemate;
            }
        }
    }
}
