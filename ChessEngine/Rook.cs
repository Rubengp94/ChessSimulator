using System;
using System.Collections.Generic;

namespace ChessEngine
{
    public class Rook : Piece
    {
        public bool HasMoved { get; set; } = false;

        public Rook(bool isWhite, Position startPosition)
            : base(isWhite, "Rook", startPosition) { }

        public override List<Move> GetValidMoves(Board board)
        {
            List<Move> validMoves = new List<Move>();
            var movements = MasterTables.PieceMovementTable["Rook"];  // Obtener los movimientos desde la tabla maestra

            foreach (var movement in movements)
            {
                int newRow = CurrentPosition.Row + movement.Item1;
                int newCol = CurrentPosition.Column + movement.Item2;
                Position newPosition = new Position(newRow, newCol);

                // Mientras la posición sea válida y no esté ocupada por piezas amigas
                while (board.IsPositionValid(newPosition) && !board.IsPositionOccupied(newPosition))
                {
                    validMoves.Add(new Move(CurrentPosition, newPosition));

                    // Si la posición está ocupada por una pieza enemiga, capturarla y detener el movimiento
                    if (board.IsPositionOccupiedByEnemyPiece(newPosition, IsWhite))
                    {
                        validMoves.Add(new Move(CurrentPosition, newPosition));
                        break;
                    }

                    // Continuar moviéndose en la misma dirección
                    newRow += movement.Item1;
                    newCol += movement.Item2;
                    newPosition = new Position(newRow, newCol);
                }
            }

            return validMoves;
        }
    }
}

