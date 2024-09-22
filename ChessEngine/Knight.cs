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

                if (board.IsPositionValid(newPos))
                {
                    // Permitir capturas si la casilla está ocupada por una pieza enemiga
                    if (!board.IsPositionOccupied(newPos) || board.IsPositionOccupiedByEnemyPiece(newPos, IsWhite))
                    {
                        validMoves.Add(new Move(CurrentPosition, newPos));
                        Console.WriteLine($"Movimiento válido para caballo hacia {newPos.Row}, {newPos.Column}");
                    }
                    else
                    {
                        Console.WriteLine($"Posición {newPos.Row}, {newPos.Column} ocupada por una pieza aliada.");
                    }
                }
                else
                {
                    Console.WriteLine($"Posición {newPos.Row}, {newPos.Column} fuera de los límites.");
                }
            }

            // Depuración
            Console.WriteLine($"Caballo en ({CurrentPosition.Row}, {CurrentPosition.Column}) movimientos válidos: {validMoves.Count}");

            return validMoves;
        }
    }
}

