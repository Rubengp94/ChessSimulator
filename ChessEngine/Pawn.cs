using System;
using System.Collections.Generic;

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

            Console.WriteLine($"Evaluando movimientos válidos para peón en {CurrentPosition.Row}, {CurrentPosition.Column}");

            // Movimiento hacia adelante (sin captura)
            Position forward = new Position(CurrentPosition.Row + direction, CurrentPosition.Column);
            if (board.IsPositionValid(forward) && !board.IsPositionOccupied(forward))
            {
                validMoves.Add(new Move(CurrentPosition, forward));
                Console.WriteLine($"Movimiento hacia adelante a {forward.Row}, {forward.Column}");
            }

            // Movimiento inicial de dos casillas hacia adelante
            if (!hasMoved && ((IsWhite && CurrentPosition.Row == 1) || (!IsWhite && CurrentPosition.Row == 6)))
            {
                Position doubleForward = new Position(CurrentPosition.Row + 2 * direction, CurrentPosition.Column);
                Position singleForward = new Position(CurrentPosition.Row + direction, CurrentPosition.Column);  // Casilla intermedia

                if (board.IsPositionValid(doubleForward) &&
                    !board.IsPositionOccupied(singleForward) &&
                    !board.IsPositionOccupied(doubleForward))
                {
                    validMoves.Add(new Move(CurrentPosition, doubleForward));
                    CanBeCapturedEnPassant = true;  // Permitir captura al paso solo tras este movimiento
                    Console.WriteLine($"Movimiento inicial doble a {doubleForward.Row}, {doubleForward.Column}");
                }
            }

            // Capturas diagonales
            var captureMoves = IsWhite ? MasterTables.PawnCaptureMoves["PawnWhite"] : MasterTables.PawnCaptureMoves["PawnBlack"];
            foreach (var captureMove in captureMoves)
            {
                Position diagonal = new Position(CurrentPosition.Row + direction, CurrentPosition.Column + captureMove.Item2); // Solo las diagonales de captura
                if (board.IsPositionValid(diagonal))
                {
                    Console.WriteLine($"Evaluando captura en diagonal a {diagonal.Row}, {diagonal.Column}");
                    if (board.IsPositionOccupiedByEnemyPiece(diagonal, IsWhite))
                    {
                        validMoves.Add(new Move(CurrentPosition, diagonal));
                        Console.WriteLine($"Captura diagonal válida hacia {diagonal.Row}, {diagonal.Column}");
                    }
                    else
                    {
                        Console.WriteLine($"No hay pieza enemiga en {diagonal.Row}, {diagonal.Column}");
                    }
                }
            }

            // Captura al paso
            HandleEnPassantCapture(board, validMoves, direction);

            // Filtrar movimientos antes de la promoción (solo agregamos promociones cuando lleguen a la fila final)
            validMoves = FilterPromotions(validMoves);

            return validMoves;
        }

        public override void AfterMove()
        {
            hasMoved = true;  // Indicar que el peón ha sido movido
        }

        private void HandleEnPassantCapture(Board board, List<Move> validMoves, int direction)
        {
            // Verificar la posibilidad de captura al paso en la columna izquierda
            if (CurrentPosition.Column > 0)
            {
                Piece leftPiece = board.GetPieceAtPosition(CurrentPosition.Row, CurrentPosition.Column - 1);
                if (leftPiece is Pawn && leftPiece.IsWhite != IsWhite && ((Pawn)leftPiece).CanBeCapturedEnPassant)
                {
                    Position enPassantLeft = new Position(CurrentPosition.Row + direction, CurrentPosition.Column - 1);
                    validMoves.Add(new Move(CurrentPosition, enPassantLeft));  // Captura al paso
                    Console.WriteLine($"Captura al paso en la izquierda en {enPassantLeft.Row}, {enPassantLeft.Column}");
                }
            }

            // Verificar la posibilidad de captura al paso en la columna derecha
            if (CurrentPosition.Column < 7)
            {
                Piece rightPiece = board.GetPieceAtPosition(CurrentPosition.Row, CurrentPosition.Column + 1);
                if (rightPiece is Pawn && rightPiece.IsWhite != IsWhite && ((Pawn)rightPiece).CanBeCapturedEnPassant)
                {
                    Position enPassantRight = new Position(CurrentPosition.Row + direction, CurrentPosition.Column + 1);
                    validMoves.Add(new Move(CurrentPosition, enPassantRight));  // Captura al paso
                    Console.WriteLine($"Captura al paso en la derecha en {enPassantRight.Row}, {enPassantRight.Column}");
                }
            }
        }

        // Filtrar los movimientos que requieren promoción y devolver una lista de movimientos
        private List<Move> FilterPromotions(List<Move> moves)
        {
            List<Move> promotionMoves = new List<Move>();
            foreach (var move in moves)
            {
                if ((IsWhite && move.End.Row == 7) || (!IsWhite && move.End.Row == 0))
                {
                    promotionMoves.Add(move);  // Solo agregamos el movimiento si requiere promoción
                }
                else
                {
                    promotionMoves.Add(move);  // Otros movimientos se agregan sin promoción
                }
            }
            return promotionMoves;
        }

        public void Promote(Board board, Position newPosition)
        {
            // Aquí agregamos lógica para que sea interactivo si se desea promover a otra cosa
            Piece newQueen = new Queen(IsWhite, newPosition);
            board.PlacePiece(newQueen, newPosition.Row, newPosition.Column);
            Console.WriteLine($"Peón promocionado a reina en la posición {newPosition.Row}, {newPosition.Column}");
        }
    }
}



