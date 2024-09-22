using ChessEngine;

public class Queen : Piece
{
    public Queen(bool isWhite, Position startPosition)
        : base(isWhite, "Queen", startPosition) { }

    public override List<Move> GetValidMoves(Board board)
    {
        List<Move> validMoves = new List<Move>();
        var movements = MasterTables.PieceMovementTable["Queen"];

        foreach (var movement in movements)
        {
            validMoves.AddRange(GetMovesInLine(board, movement.Item1, movement.Item2));
        }

        return validMoves;
    }
}
