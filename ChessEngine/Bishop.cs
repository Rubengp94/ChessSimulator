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
            validMoves.AddRange(GetMovesInLine(board, movement.Item1, movement.Item2));  // Pasar las direcciones directamente
        }

        return validMoves;
    }
}
