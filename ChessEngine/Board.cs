using System;
using System.Diagnostics;

namespace ChessEngine
{
    public class Board
    {
        public const int Size = 8;
        public Piece?[,] Grid;  // Matriz 8x8 de referencias a piezas (que pueden ser nulas)

        public Board()
        {
            // Inicializamos el tablero 8x8
            Grid = new Piece?[Size, Size];
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    Grid[row, col] = null;  // Inicializamos con posiciones vacías
                }
            }
        }

        // Obtener una referencia a la pieza en una posición específica
        public Piece? GetPieceAtPosition(int row, int col)
        {
            if (row < 0 || row >= Size || col < 0 || col >= Size)
            {
                return null;  // Posición fuera del tablero
            }
            return Grid[row, col];  // Retorna la referencia a la pieza o null si no hay ninguna
        }

        // Colocar una pieza en una posición específica utilizando una referencia
        public void PlacePiece(Piece piece, int row, int col)
        {
            Debug.WriteLine($"Colocando {piece.PieceType} en ({row}, {col})");
            Grid[row, col] = piece;  // Coloca la pieza en la posición
        }

        // Mover una pieza de una posición a otra
        public Piece? MovePiece(int startRow, int startCol, int endRow, int endCol)
        {
            Debug.WriteLine($"Intentando mover pieza desde ({startRow}, {startCol}) a ({endRow}, {endCol})");

            // Validar posiciones antes de mover usando IsPositionValid
            Position startPosition = new Position(startRow, startCol);
            Position endPosition = new Position(endRow, endCol);

            if (!IsPositionValid(startPosition) || !IsPositionValid(endPosition))
            {
                Debug.WriteLine($"Error: Posición fuera de rango al intentar mover desde ({startRow}, {startCol}) a ({endRow}, {endCol})");
                return null;
            }

            Piece? pieceToMove = GetPieceAtPosition(startRow, startCol);
            if (pieceToMove == null)
            {
                Debug.WriteLine($"Error: No se encontró ninguna pieza en la posición de origen ({startRow}, {startCol})");
                return null;
            }

            Piece? capturedPiece = GetPieceAtPosition(endRow, endCol);

            // Movemos la pieza
            Grid[startRow, startCol] = null;  // Eliminar la pieza de la posición original
            Grid[endRow, endCol] = pieceToMove;  // Colocar la pieza en la nueva posición
            pieceToMove.CurrentPosition = endPosition;  // Actualizar la posición actual

            Debug.WriteLine($"Pieza {pieceToMove.PieceType} movida a la nueva posición ({endRow}, {endCol})");

            return capturedPiece;
        }


        // Verificar si una posición de tipo `Position` está dentro de los límites del tablero
        public bool IsPositionValid(Position position)
        {
            return position.Row >= 0 && position.Row < Board.Size && position.Column >= 0 && position.Column < Board.Size;
        }



        // Verificar si una casilla está ocupada por una pieza
        public bool IsPositionOccupied(Position position)
        {
            return GetPieceAtPosition(position.Row, position.Column) != null;
        }

        // Verificar si una posición está ocupada por una pieza del equipo contrario
        public bool IsPositionOccupiedByEnemyPiece(Position position, bool isWhite)
        {
            Piece? piece = GetPieceAtPosition(position.Row, position.Column);
            return piece != null && piece.IsWhite != isWhite;
        }

        // Método auxiliar para imprimir el estado actual del tablero
        public void PrintBoardState()
        {
            Console.WriteLine("Estado del tablero:");
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    Piece? piece = Grid[row, col];
                    if (piece != null)
                    {
                        Console.Write($"{piece.PieceType[0]} ");  // Mostrar el tipo de pieza con su primera letra
                    }
                    else
                    {
                        Console.Write(". ");  // Mostrar un punto para las casillas vacías
                    }
                }
                Console.WriteLine();  // Nueva línea al final de cada fila
            }
            Console.WriteLine();  // Espacio adicional entre impresiones
        }
    }
}





