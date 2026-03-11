namespace ChessBackend.Models
{
    public enum PieceColor
    {
        White,
        Black
    }

    public enum PieceType
    {
        Pawn,
        Rook,
        Knight,
        Bishop,
        Queen,
        King
    }

    public record Position(int File, int Rank)
    {
        public bool IsValid() => File >= 0 && File < 8 && Rank >= 0 && Rank < 8;
        
        public static Position FromString(string pos) 
        {
            if (pos.Length != 2) throw new ArgumentException("Invalid position format");
            int file = pos[0] - 'a';
            int rank = pos[1] - '1';
            return new Position(file, rank);
        }

        public override string ToString() => $"{(char)('a' + File)}{Rank + 1}";
    }
}
