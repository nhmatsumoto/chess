namespace ChessBackend.Logic
{
    using Models;

    public class MoveValidator
    {
        public static bool IsValidMove(ChessGame game, Position from, Position to)
        {
            var piece = game.Board.GetPiece(from);
            if (piece == null || piece.Color != game.CurrentTurn) return false;

            var targetPiece = game.Board.GetPiece(to);
            if (targetPiece != null && targetPiece.Color == piece.Color) return false;

            int dx = to.File - from.File;
            int dy = to.Rank - from.Rank;

            return piece.Type switch
            {
                PieceType.Pawn => ValidatePawn(game, piece, from, to, dx, dy),
                PieceType.Rook => ValidateRook(game, from, to, dx, dy),
                PieceType.Knight => ValidateKnight(dx, dy),
                PieceType.Bishop => ValidateBishop(game, from, to, dx, dy),
                PieceType.Queen => ValidateQueen(game, from, to, dx, dy),
                PieceType.King => ValidateKing(dx, dy),
                _ => false
            };
        }

        private static bool ValidatePawn(ChessGame game, ChessPiece piece, Position from, Position to, int dx, int dy)
        {
            int direction = piece.Color == PieceColor.White ? 1 : -1;
            var target = game.Board.GetPiece(to);

            // Forward move
            if (dx == 0)
            {
                if (target != null) return false;
                if (dy == direction) return true;
                if (!piece.HasMoved && dy == 2 * direction)
                {
                    return game.Board.GetPiece(new Position(from.File, from.Rank + direction)) == null;
                }
            }
            // Capture
            else if (Math.Abs(dx) == 1 && dy == direction)
            {
                return target != null && target.Color != piece.Color;
            }

            return false;
        }

        private static bool ValidateRook(ChessGame game, Position from, Position to, int dx, int dy)
        {
            if (dx != 0 && dy != 0) return false;
            return IsPathClear(game, from, to);
        }

        private static bool ValidateKnight(int dx, int dy)
        {
            int adx = Math.Abs(dx);
            int ady = Math.Abs(dy);
            return (adx == 1 && ady == 2) || (adx == 2 && ady == 1);
        }

        private static bool ValidateBishop(ChessGame game, Position from, Position to, int dx, int dy)
        {
            if (Math.Abs(dx) != Math.Abs(dy)) return false;
            return IsPathClear(game, from, to);
        }

        private static bool ValidateQueen(ChessGame game, Position from, Position to, int dx, int dy)
        {
            if (dx != 0 && dy != 0 && Math.Abs(dx) != Math.Abs(dy)) return false;
            return IsPathClear(game, from, to);
        }

        private static bool ValidateKing(int dx, int dy)
        {
            return Math.Abs(dx) <= 1 && Math.Abs(dy) <= 1;
        }

        private static bool IsPathClear(ChessGame game, Position from, Position to)
        {
            int xStep = Math.Sign(to.File - from.File);
            int yStep = Math.Sign(to.Rank - from.Rank);
            int x = from.File + xStep;
            int y = from.Rank + yStep;

            while (x != to.File || y != to.Rank)
            {
                if (game.Board.GetPiece(new Position(x, y)) != null) return false;
                x += xStep;
                y += yStep;
            }
            return true;
        }
    }
}
