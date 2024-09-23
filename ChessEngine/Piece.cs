using System;
using System.Collections.Generic;

namespace ChessEngine
{
    public abstract class Piece
    {
        public bool IsWhite { get; private set; }  // Indica si la pieza es blanca o negra
        public string PieceType { get; private set; }  // Tipo de pieza (Reina, Torre, etc.)
        public Position CurrentPosition { get; set; }  // Posición actual de la pieza

        // Método que se llama después de mover la pieza
        public virtual void AfterMove()
        {
            // El método puede ser sobrescrito por piezas específicas si es necesario
        }

        public virtual void AfterMoveUndo()
        {
            // En general, no hay nada que revertir para la mayoría de las piezas,
            // pero las piezas como los peones pueden sobrescribirlo si es necesario.
        }

        protected Piece(bool isWhite, string pieceType, Position startPosition)
        {
            if (startPosition == null)
            {
                throw new ArgumentNullException(nameof(startPosition), "La posición inicial no puede ser nula.");
            }

            IsWhite = isWhite;
            PieceType = pieceType;
            CurrentPosition = startPosition;  // Asegurarse de que la posición se inicializa correctamente
            Console.WriteLine($"Pieza {PieceType} {(IsWhite ? "blanca" : "negra")} colocada en {startPosition.Row}, {startPosition.Column}");
        }

        // Método abstracto para obtener los movimientos válidos
        public abstract List<Move> GetValidMoves(Board board);

        // Método para obtener los movimientos en línea (para la torre, reina, alfil)
        protected List<Move> GetMovesInLine(Board board, int rowDir, int colDir)
        {
            List<Move> moves = new List<Move>();
            int newRow = CurrentPosition.Row + rowDir;
            int newCol = CurrentPosition.Column + colDir;

            Console.WriteLine($"Evaluando movimientos en línea para {PieceType} desde {CurrentPosition.Row}, {CurrentPosition.Column}");

            while (board.IsPositionValid(new Position(newRow, newCol)))
            {
                Position newPos = new Position(newRow, newCol);
                Console.WriteLine($"Evaluando posición {newPos.Row}, {newPos.Column}");

                if (board.IsPositionOccupied(newPos))
                {
                    if (board.IsPositionOccupiedByEnemyPiece(newPos, IsWhite))
                    {
                        moves.Add(new Move(CurrentPosition, newPos));
                        Console.WriteLine($"Movimiento de captura válido hacia {newPos.Row}, {newPos.Column}");
                    }
                    break;  // Se detiene el movimiento si encuentra una pieza.
                }

                moves.Add(new Move(CurrentPosition, newPos));
                Console.WriteLine($"Movimiento en línea válido hacia {newPos.Row}, {newPos.Column}");

                newRow += rowDir;
                newCol += colDir;
            }

            return moves;
        }


        // Método para obtener movimientos en "L" (Caballo)
        protected List<Move> GetKnightMoves(Board board)
        {
            List<Move> moves = new List<Move>();
            var knightMoves = MasterTables.PieceMovementTable["Knight"]; // Usar tabla maestra
            Console.WriteLine($"Evaluando movimientos de caballo para {PieceType} desde {CurrentPosition.Row}, {CurrentPosition.Column}");

            foreach (var move in knightMoves)
            {
                Position newPosition = new Position(CurrentPosition.Row + move.Item1, CurrentPosition.Column + move.Item2);

                if (board.IsPositionValid(newPosition))
                {
                    if (!board.IsPositionOccupied(newPosition) || board.IsPositionOccupiedByEnemyPiece(newPosition, IsWhite))
                    {
                        moves.Add(new Move(CurrentPosition, newPosition));
                        Console.WriteLine($"Movimiento de caballo válido hacia {newPosition.Row}, {newPosition.Column}");
                    }
                    else
                    {
                        Console.WriteLine($"Posición {newPosition.Row}, {newPosition.Column} ocupada por una pieza aliada.");
                    }
                }
                else
                {
                    Console.WriteLine($"Posición {newPosition.Row}, {newPosition.Column} fuera de los límites.");
                }
            }

            return moves;
        }
    }
}

