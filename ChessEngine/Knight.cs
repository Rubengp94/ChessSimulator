using System;
using System.Collections.Generic;

namespace ChessEngine
{
    public class Knight : Piece
    {
        public Knight(bool isWhite, Position startPosition)
            : base(isWhite, "Knight", startPosition) { }

        public override List<Move> GetValidMoves(Board board)
        {
            List<Move> validMoves = new List<Move>();
            var movements = MasterTables.PieceMovementTable["Knight"];

            foreach (var move in movements)
            {
                int newRow = CurrentPosition.Row + move.Item1;
                int newCol = CurrentPosition.Column + move.Item2;
                Position newPos = new Position(newRow, newCol);

                if (board.IsPositionValid(newPos) && !board.IsPositionOccupiedByEnemyPiece(newPos, IsWhite))
                {
                    validMoves.Add(new Move(CurrentPosition, newPos));
                }
            }

            // Depuración
            Console.WriteLine($"Knight ({CurrentPosition.Row}, {CurrentPosition.Column}) movimientos válidos: {validMoves.Count}");

            return validMoves;
        }
    }
}
