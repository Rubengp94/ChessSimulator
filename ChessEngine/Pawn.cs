﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ChessEngine
{
    public class Pawn : Piece
    {
        public bool CanBeCapturedEnPassant { get; set; } = false;  // Indica si este peón puede ser capturado al paso
        private bool hasMoved = false;  // Indica si el peón ha sido movido

        public Pawn(bool isWhite, Position startPosition)
            : base(isWhite, "Pawn", startPosition) { }

        public override List<Move> GetValidMoves(Board board)
        {
            List<Move> validMoves = new List<Move>();
            int direction = IsWhite ? 1 : -1;  // Blancas suben (+1), negras bajan (-1)

            // Movimiento hacia adelante
            Position forward = new Position(CurrentPosition.Row + direction, CurrentPosition.Column);
            if (board.IsPositionValid(forward) && !board.IsPositionOccupied(forward))
            {
                validMoves.Add(new Move(CurrentPosition, forward));
            }

            // Movimiento doble al inicio
            if (!hasMoved && ((IsWhite && CurrentPosition.Row == 1) || (!IsWhite && CurrentPosition.Row == 6)))
            {
                Position doubleForward = new Position(CurrentPosition.Row + 2 * direction, CurrentPosition.Column);
                Position singleForward = new Position(CurrentPosition.Row + direction, CurrentPosition.Column);

                if (board.IsPositionValid(doubleForward) &&
                    !board.IsPositionOccupied(singleForward) &&
                    !board.IsPositionOccupied(doubleForward))
                {
                    validMoves.Add(new Move(CurrentPosition, doubleForward));
                }
            }

            // Captura diagonal
            var captureMoves = IsWhite ? MasterTables.PawnCaptureMoves["PawnWhite"] : MasterTables.PawnCaptureMoves["PawnBlack"];
            foreach (var captureMove in captureMoves)
            {
                Position diagonal = new Position(CurrentPosition.Row + direction, CurrentPosition.Column + captureMove.Item2);
                if (board.IsPositionValid(diagonal) && board.IsPositionOccupiedByEnemyPiece(diagonal, IsWhite))
                {
                    validMoves.Add(new Move(CurrentPosition, diagonal));
                }
            }

            // Captura al paso
            HandleEnPassantCapture(board, validMoves, direction);

            return FilterPromotions(validMoves);
        }

        public override void AfterMove()
        {
            hasMoved = true;
        }

        private void HandleEnPassantCapture(Board board, List<Move> validMoves, int direction)
        {
            if (CurrentPosition.Column > 0)
            {
                Piece? leftPiece = board.GetPieceAtPosition(CurrentPosition.Row, CurrentPosition.Column - 1);
                if (leftPiece is Pawn && leftPiece.IsWhite != IsWhite && ((Pawn)leftPiece).CanBeCapturedEnPassant)
                {
                    Position enPassantLeft = new Position(CurrentPosition.Row + direction, CurrentPosition.Column - 1);
                    if (board.IsPositionValid(enPassantLeft) && !board.IsPositionOccupied(enPassantLeft))
                    {
                        validMoves.Add(new Move(CurrentPosition, enPassantLeft));  // Solo si la casilla está vacía
                    }
                }
            }

            if (CurrentPosition.Column < 7)
            {
                Piece? rightPiece = board.GetPieceAtPosition(CurrentPosition.Row, CurrentPosition.Column + 1);
                if (rightPiece is Pawn && rightPiece.IsWhite != IsWhite && ((Pawn)rightPiece).CanBeCapturedEnPassant)
                {
                    Position enPassantRight = new Position(CurrentPosition.Row + direction, CurrentPosition.Column + 1);
                    if (board.IsPositionValid(enPassantRight) && !board.IsPositionOccupied(enPassantRight))
                    {
                        validMoves.Add(new Move(CurrentPosition, enPassantRight));  // Solo si la casilla está vacía
                    }
                }
            }
        }

        private List<Move> FilterPromotions(List<Move> moves)
        {
            List<Move> promotionMoves = new List<Move>();
            foreach (var move in moves)
            {
                // Solo permitir promociones cuando el peón blanco llegue a la fila 7 o el peón negro a la fila 0
                if ((IsWhite && move.End.Row == 7) || (!IsWhite && move.End.Row == 0))
                {
                    promotionMoves.Add(move);  // Solo agregar si está en la fila de promoción
                }
                else
                {
                    promotionMoves.Add(move);  // Otros movimientos regulares
                }
            }
            return promotionMoves;
        }

        public void Promote(Board board, Position newPosition)
        {
            Piece newQueen = new Queen(IsWhite, newPosition);  // Crear la nueva reina
            board.PlacePiece(newQueen, newPosition.Row, newPosition.Column);  // Colocar la reina en el tablero
            Debug.WriteLine($"Peón promocionado a reina en la posición {newPosition.Row}, {newPosition.Column}");
        }
    }
}





