using System;
using System.Collections.Generic;

namespace ChessEngine
{
    public class King : Piece
    {
        public bool HasMoved { get; set; } = false;

        public King(bool isWhite, Position startPosition)
            : base(isWhite, "King", startPosition) { }

        public override List<Move> GetValidMoves(Board board)
        {
            List<Move> validMoves = new List<Move>();
            var movements = MasterTables.PieceMovementTable["King"];  // Obtener los movimientos del Rey desde la tabla maestra

            foreach (var movement in movements)
            {
                int newRow = CurrentPosition.Row + movement.Item1;
                int newCol = CurrentPosition.Column + movement.Item2;
                Position newPosition = new Position(newRow, newCol);

                // Si la posición es válida y no está ocupada por una pieza amiga, agregar el movimiento
                if (board.IsPositionValid(newPosition) && !board.IsPositionOccupied(newPosition))
                {
                    validMoves.Add(new Move(CurrentPosition, newPosition));
                }

                // Si la posición está ocupada por una pieza enemiga, se puede capturar
                if (board.IsPositionValid(newPosition) && board.IsPositionOccupiedByEnemyPiece(newPosition, IsWhite))
                {
                    validMoves.Add(new Move(CurrentPosition, newPosition));
                }
            }

            // Considerar movimientos especiales como el enroque
            if (!HasMoved)
            {
                validMoves.AddRange(GetCastlingMoves(board));
            }

            return validMoves;
        }

        // Método para agregar los movimientos de enroque y mover la torre también
        private List<Move> GetCastlingMoves(Board board)
        {
            List<Move> castlingMoves = new List<Move>();

            // Enroque corto (lado del rey)
            if (CanCastle(board, 7, 5, 6))  // Columnas 5 y 6 entre el rey y la torre
            {
                // Mover el rey a la columna 6
                castlingMoves.Add(new Move(CurrentPosition, new Position(CurrentPosition.Row, 6)));

                // Mover la torre a la columna 5
                Piece rook = board.GetPieceAtPosition(CurrentPosition.Row, 7);
                if (rook is Rook && !((Rook)rook).HasMoved)
                {
                    castlingMoves.Add(new Move(rook.CurrentPosition, new Position(CurrentPosition.Row, 5)));  // Mover la torre a la columna 5
                }
            }

            // Enroque largo (lado de la reina)
            if (CanCastle(board, 0, 1, 2, 3))  // Columnas 1, 2 y 3 entre el rey y la torre
            {
                // Mover el rey a la columna 2
                castlingMoves.Add(new Move(CurrentPosition, new Position(CurrentPosition.Row, 2)));

                // Mover la torre a la columna 3
                Piece rook = board.GetPieceAtPosition(CurrentPosition.Row, 0);
                if (rook is Rook && !((Rook)rook).HasMoved)
                {
                    castlingMoves.Add(new Move(rook.CurrentPosition, new Position(CurrentPosition.Row, 3)));  // Mover la torre a la columna 3
                }
            }

            return castlingMoves;
        }


        // Verificar si el enroque es posible
        private bool CanCastle(Board board, int rookCol, params int[] emptyCols)
        {
            // Verificar si las casillas entre el rey y la torre están vacías
            foreach (int col in emptyCols)
            {
                if (board.IsPositionOccupied(new Position(CurrentPosition.Row, col)))
                {
                    return false;  // No se puede enrocar si hay piezas en el camino
                }
            }

            // Verificar que la torre no se haya movido
            Piece rook = board.GetPieceAtPosition(CurrentPosition.Row, rookCol);
            if (rook is Rook && !((Rook)rook).HasMoved && rook.IsWhite == IsWhite)
            {
                return true;  // Se puede enrocar si la torre no se ha movido y es del mismo color
            }

            return false;  // No se puede enrocar
        }
    }
}



