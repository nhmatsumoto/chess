namespace ChessBackend.Models
{
    public class ChessGame
    {
        public ChessBoard Board { get; }
        public PieceColor CurrentTurn { get; private set; } = PieceColor.White;
        public List<string> MoveHistory { get; } = new List<string>();

        public ChessGame()
        {
            Board = new ChessBoard();
            Board.InitializeStandard();
        }

        public bool ExecuteMove(Position from, Position to)
        {
            // Simple execution for now, validation will come later
            var piece = Board.GetPiece(from);
            if (piece == null || piece.Color != CurrentTurn) return false;

            Board.SetPiece(to, piece);
            Board.SetPiece(from, null);
            piece.HasMoved = true;

            MoveHistory.Add($"{from} to {to}");
            CurrentTurn = CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
            return true;
        }
    }
}
