using ChessEngine;

public class Bishop : Piece
{
    public Bishop(bool isWhite, Position startPosition)
        : base(isWhite, "Bishop", startPosition) { }

    public override List<Move> GetValidMoves(Board board)
    {
        List<Move> validMoves = new List<Move>();
        var movements = MasterTables.PieceMovementTable["Bishop"];

        foreach (var movement in movements)
        {
            int newRow = CurrentPosition.Row + movement.Item1;
            int newCol = CurrentPosition.Column + movement.Item2;
            validMoves.AddRange(GetMovesInLine(board, newRow, newCol));
        }

        return validMoves;
    }
}
