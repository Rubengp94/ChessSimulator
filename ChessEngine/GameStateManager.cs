using System;
using System.Diagnostics;

namespace ChessEngine
{
    public partial class GameStateManager
    {
        private Board board;

        public GameStateManager(Board board)
        {
            this.board = board;
        }

        // Verifica si el jugador está en jaque mate o tablas (o si el juego ha terminado de otra manera)
        public bool IsGameOver()
        {
            return IsCheckMate(true) || IsCheckMate(false) || IsStalemate(true) || IsStalemate(false);
        }

        // Verifica si el jugador está en jaque
        public bool IsCheck(bool isWhite)
        {
            Position? kingPosition = GetKingPosition(isWhite);
            if (kingPosition == null) return false; // Si no hay rey, no se puede estar en jaque.
            return IsSquareUnderAttack(kingPosition, !isWhite);  // Verifica si el rey está bajo ataque
        }

        // Comprueba si un movimiento deja al rey del mismo color en jaque
        public bool IsMoveInCheck(Move move, bool isWhite)
        {
            // Simular el movimiento
            Piece? capturedPiece = board.MovePiece(move.Start.Row, move.Start.Column, move.End.Row, move.End.Column);
            bool inCheck = IsCheck(isWhite);  // Verificar si el rey está en jaque después del movimiento

            // Deshacer el movimiento
            board.MovePiece(move.End.Row, move.End.Column, move.Start.Row, move.Start.Column);
            if (capturedPiece != null)
            {
                board.PlacePiece(capturedPiece, move.End.Row, move.End.Column);
            }

            return inCheck;  // Devuelve true si el movimiento deja al rey en jaque
        }

        // Comprueba si hay jaque mate para el jugador dado
        public bool IsCheckMate(bool isWhite)
        {
            if (!IsCheck(isWhite)) return false;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece? piece = board.GetPieceAtPosition(row, col);
                    if (piece != null && piece.IsWhite == isWhite)
                    {
                        var validMoves = piece.GetValidMoves(board);
                        foreach (var move in validMoves)
                        {
                            if (!IsMoveInCheck(move, isWhite))
                            {
                                return false;  // No es jaque mate si hay un movimiento válido
                            }
                        }
                    }
                }
            }
            return true;  // Es jaque mate si no hay movimientos válidos
        }


        // Comprueba si hay tablas (estancamiento) para el jugador dado
        public bool IsStalemate(bool isWhite)
        {
            if (IsCheck(isWhite)) return false;  // Si está en jaque, no puede haber tablas

            // Recorrer todas las piezas del jugador para ver si tiene algún movimiento válido
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece? piece = board.GetPieceAtPosition(row, col);
                    if (piece != null && piece.IsWhite == isWhite)
                    {
                        var validMoves = piece.GetValidMoves(board);
                        foreach (var move in validMoves)
                        {
                            if (!IsMoveInCheck(move, isWhite))
                            {
                                return false;  // Si hay algún movimiento válido que no deja al rey en jaque, no es tablas
                            }
                        }
                    }
                }
            }
            return true;  // Si no hay movimientos válidos, es tablas
        }

        // Verifica si una casilla está siendo atacada por una pieza enemiga
        private bool IsSquareUnderAttack(Position position, bool isOpponentWhite)
        {
            // Revisar todas las piezas enemigas y ver si alguna puede atacar la posición dada
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece? piece = board.GetPieceAtPosition(row, col);
                    if (piece != null && piece.IsWhite == isOpponentWhite)
                    {
                        var validMoves = piece.GetValidMoves(board);
                        foreach (var move in validMoves)
                        {
                            if (move.End.Row == position.Row && move.End.Column == position.Column)
                            {
                                return true;  // La casilla está bajo ataque
                            }
                        }
                    }
                }
            }
            return false;  // Si ninguna pieza enemiga puede atacar la casilla, no está bajo ataque
        }

        // Obtiene la posición del rey del jugador dado
        public Position? GetKingPosition(bool isWhite)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece? piece = board.GetPieceAtPosition(row, col);
                    if (piece != null && piece is King && piece.IsWhite == isWhite)
                    {
                        Debug.WriteLine($"Rey {(isWhite ? "blanco" : "negro")} encontrado en ({row}, {col})");
                        return new Position(row, col); // Devolver la posición del rey
                    }
                }
            }
            Debug.WriteLine($"Advertencia: Rey {(isWhite ? "blanco" : "negro")} no encontrado en el tablero");
            return null;  // No se encontró el rey, devolver null
        }
    }
}
