using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ChessEngine
{
    public class ChessAI
    {
        private const int MaxDepth = 3;  // Mantener la profundidad para una evaluación más precisa
        private Board board;
        private GameStateManager gameStateManager;

        public ChessAI(Board board, GameStateManager gameStateManager)
        {
            this.board = board;
            this.gameStateManager = gameStateManager;
        }

        public Move GetBestMove(bool isWhite)
        {
            int bestScore = int.MinValue;
            Move bestMove = null;

            List<Move> allMoves = GetAllValidMoves(isWhite);
            Debug.WriteLine($"Total de movimientos válidos para {(isWhite ? "blancas" : "negras")}: {allMoves.Count}");

            // Filtrar movimientos que saquen del jaque
            allMoves = FilterOutMovesThatLeaveInCheck(allMoves, isWhite);

            // Ordenar los movimientos, priorizando las capturas
            allMoves = SortMovesBySignificance(allMoves);

            foreach (var move in allMoves)
            {
                Piece piece = board.GetPieceAtPosition(move.Start.Row, move.Start.Column);
                if (piece == null)
                {
                    Debug.WriteLine($"Error: No se encontró pieza en la posición ({move.Start.Row}, {move.Start.Column})");
                    continue;
                }

                Debug.WriteLine($"Evaluando movimiento de {piece.PieceType} desde ({move.Start.Row}, {move.Start.Column}) a ({move.End.Row}, {move.End.Column})");

                // Mover la pieza temporalmente
                Piece? capturedPiece = board.MovePiece(move.Start.Row, move.Start.Column, move.End.Row, move.End.Column);

                // Simular promoción solo si es un peón negro y está moviéndose a la fila 0 (no antes)
                Piece? promotedPiece = null;
                Pawn? pawn = piece as Pawn;
                bool promotionHappened = false;

                // Verificar promoción solo para el movimiento final, no en simulaciones intermedias
                if (pawn != null && !pawn.IsWhite && move.End.Row == 0)
                {
                    promotedPiece = new Queen(pawn.IsWhite, move.End);  // Simulamos la promoción a reina
                    board.PlacePiece(promotedPiece, move.End.Row, move.End.Column);
                    promotionHappened = true;
                    Debug.WriteLine($"Simulando promoción temporal de peón negro a reina en ({move.End.Row}, {move.End.Column})");
                }

                piece.AfterMove();  // Actualizar el estado de la pieza si es necesario

                // Evaluar el movimiento usando Minimax con Quiescence Search
                int moveScore = QuiescenceSearch(MaxDepth, int.MinValue, int.MaxValue, !isWhite);

                // Deshacer el movimiento (revertir la simulación)
                board.MovePiece(move.End.Row, move.End.Column, move.Start.Row, move.Start.Column);
                piece.AfterMoveUndo();  // Revertir el estado de `hasMoved`

                // Restaurar la pieza capturada
                if (capturedPiece != null)
                {
                    board.PlacePiece(capturedPiece, move.End.Row, move.End.Column);
                }

                // Revertir la promoción temporal si se había promovido
                if (promotionHappened)
                {
                    board.PlacePiece(pawn, move.End.Row, move.End.Column);  // Volver a colocar el peón
                    Debug.WriteLine($"Revirtiendo la promoción temporal de la reina a peón en ({move.End.Row}, {move.End.Column})");
                }

                // Añadir el valor de la pieza capturada al puntaje
                if (capturedPiece != null)
                {
                    moveScore += GetPieceValue(capturedPiece);
                    Debug.WriteLine($"Movimiento de captura de {capturedPiece.PieceType}, aumentando puntaje en {GetPieceValue(capturedPiece)}");
                }

                // Actualizar el mejor movimiento si encontramos un movimiento con mejor puntaje
                if (moveScore > bestScore)
                {
                    bestScore = moveScore;
                    bestMove = move;
                    Debug.WriteLine($"Nuevo mejor movimiento: {piece.PieceType} desde ({move.Start.Row}, {move.Start.Column}) a ({move.End.Row}, {move.End.Column}) con score {moveScore}");
                }
            }

            if (bestMove == null)
            {
                Debug.WriteLine("Error: No se encontró ningún movimiento válido para la IA.");
            }

            return bestMove;
        }


        // Filtrar movimientos que dejan al rey en jaque
        private List<Move> FilterOutMovesThatLeaveInCheck(List<Move> moves, bool isWhite)
        {
            List<Move> safeMoves = new List<Move>();

            foreach (var move in moves)
            {
                // Mover la pieza temporalmente
                Piece? capturedPiece = board.MovePiece(move.Start.Row, move.Start.Column, move.End.Row, move.End.Column);

                // Comprobar si el rey sigue en jaque después del movimiento
                bool isStillInCheck = gameStateManager.IsCheck(isWhite);

                // Deshacer el movimiento
                board.MovePiece(move.End.Row, move.End.Column, move.Start.Row, move.Start.Column);
                if (capturedPiece != null)
                {
                    board.PlacePiece(capturedPiece, move.End.Row, move.End.Column);
                }

                // Solo agregar el movimiento si no deja al rey en jaque
                if (!isStillInCheck)
                {
                    safeMoves.Add(move);
                }
            }

            return safeMoves;
        }


        internal int QuiescenceSearch(int depth, int alpha, int beta, bool isMaximizingPlayer)
        {
            // Evaluación estática del tablero
            int eval = EvaluateBoard();
            if (eval >= beta) return beta;
            if (eval > alpha) alpha = eval;

            // Obtener movimientos de captura
            List<Move> captureMoves = GetCaptureMoves(isMaximizingPlayer);
            if (depth == 0 || captureMoves.Count == 0) return eval;

            // Explorar solo movimientos de captura
            foreach (var move in captureMoves)
            {
                Piece piece = board.GetPieceAtPosition(move.Start.Row, move.Start.Column);
                if (piece == null) continue;

                // Mover la pieza temporalmente
                Piece? capturedPiece = board.MovePiece(move.Start.Row, move.Start.Column, move.End.Row, move.End.Column);
                piece.AfterMove();

                int score = -QuiescenceSearch(depth - 1, -beta, -alpha, !isMaximizingPlayer);

                // Deshacer el movimiento
                board.MovePiece(move.End.Row, move.End.Column, move.Start.Row, move.Start.Column);
                piece.AfterMoveUndo();
                if (capturedPiece != null) board.PlacePiece(capturedPiece, move.End.Row, move.End.Column);

                if (score >= beta) return beta;
                if (score > alpha) alpha = score;
            }
            return alpha;
        }

        internal int Minimax(int depth, int alpha, int beta, bool isMaximizingPlayer)
        {
            if (depth == 0 || gameStateManager.IsGameOver())
            {
                return EvaluateBoard();
            }

            List<Move> moves = GetAllValidMoves(isMaximizingPlayer);
            bool isInCheck = gameStateManager.IsCheck(isMaximizingPlayer);  // Verificar si está en jaque

            if (isMaximizingPlayer)
            {
                int maxEval = int.MinValue;
                foreach (var move in moves)
                {
                    Piece piece = board.GetPieceAtPosition(move.Start.Row, move.Start.Column);
                    if (piece == null) continue;

                    // Mover la pieza temporalmente
                    Piece? capturedPiece = board.MovePiece(move.Start.Row, move.Start.Column, move.End.Row, move.End.Column);
                    piece.AfterMove();  // Actualizar el estado de movimiento

                    // Simular promoción solo en el movimiento final
                    Pawn? pawn = piece as Pawn;
                    if (pawn != null && !pawn.IsWhite && move.End.Row == 0)
                    {
                        board.PlacePiece(new Queen(false, move.End), move.End.Row, move.End.Column);
                    }

                    int eval = Minimax(depth - 1, alpha, beta, false);

                    // Deshacer el movimiento
                    board.MovePiece(move.End.Row, move.End.Column, move.Start.Row, move.Start.Column);
                    piece.AfterMoveUndo();  // Revertir estado de `hasMoved`

                    // Revertir promoción de peones en simulaciones
                    if (pawn != null && move.End.Row == 0)
                    {
                        board.PlacePiece(pawn, move.End.Row, move.End.Column);  // Revertir promoción temporal
                    }

                    // Restaurar la pieza capturada
                    if (capturedPiece != null)
                    {
                        board.PlacePiece(capturedPiece, move.End.Row, move.End.Column);
                    }

                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha) break;  // Poda alfa-beta
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (var move in moves)
                {
                    Piece piece = board.GetPieceAtPosition(move.Start.Row, move.Start.Column);
                    if (piece == null) continue;

                    // Mover la pieza temporalmente
                    Piece? capturedPiece = board.MovePiece(move.Start.Row, move.Start.Column, move.End.Row, move.End.Column);
                    piece.AfterMove();  // Actualizar el estado de movimiento

                    // Simular promoción solo en el movimiento final
                    Pawn? pawn = piece as Pawn;
                    if (pawn != null && !pawn.IsWhite && move.End.Row == 0)
                    {
                        board.PlacePiece(new Queen(false, move.End), move.End.Row, move.End.Column);
                    }

                    int eval = Minimax(depth - 1, alpha, beta, true);

                    // Deshacer el movimiento
                    board.MovePiece(move.End.Row, move.End.Column, move.Start.Row, move.Start.Column);
                    piece.AfterMoveUndo();  // Revertir estado de `hasMoved`

                    // Revertir promoción de peones en simulaciones
                    if (pawn != null && move.End.Row == 0)
                    {
                        board.PlacePiece(pawn, move.End.Row, move.End.Column);  // Revertir promoción temporal
                    }

                    // Restaurar la pieza capturada
                    if (capturedPiece != null)
                    {
                        board.PlacePiece(capturedPiece, move.End.Row, move.End.Column);
                    }

                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha) break;  // Poda alfa-beta
                }
                return minEval;
            }
        }


        internal int EvaluateBoard()
        {
            int score = 0;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece? piece = board.GetPieceAtPosition(row, col);
                    if (piece != null)
                    {
                        int pieceValue = GetPieceValue(piece);
                        score += piece.IsWhite ? pieceValue : -pieceValue;

                        // Penalizar al jugador si está en jaque
                        if (piece.PieceType == "King" && gameStateManager.IsCheck(piece.IsWhite))
                        {
                            score += piece.IsWhite ? -500 : 500;  // Penaliza mucho si el rey está en jaque
                        }
                    }
                }
            }
            return score;
        }

        private int GetPieceValue(Piece piece)
        {
            switch (piece.PieceType)
            {
                case "Pawn": return 10;
                case "Knight": return 30;
                case "Bishop": return 30;
                case "Rook": return 50;
                case "Queen": return 90;
                case "King": return 900;
                default: return 0;
            }
        }

        private List<Move> GetAllValidMoves(bool isWhite)
        {
            List<Move> moves = new List<Move>();
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece? piece = board.GetPieceAtPosition(row, col);
                    if (piece != null && piece.IsWhite == isWhite)
                    {
                        List<Move> validMoves = piece.GetValidMoves(board);
                        moves.AddRange(validMoves);
                    }
                }
            }
            return moves;
        }

        private List<Move> GetCaptureMoves(bool isWhite)
        {
            List<Move> captureMoves = new List<Move>();
            foreach (var move in GetAllValidMoves(isWhite))
            {
                if (board.IsPositionOccupiedByEnemyPiece(move.End, isWhite))
                {
                    captureMoves.Add(move);  // Solo capturas
                }
            }
            return captureMoves;
        }

        private List<Move> SortMovesBySignificance(List<Move> moves)
        {
            // Ordenar los movimientos de captura primero (mayor valor primero)
            moves.Sort((move1, move2) =>
            {
                Piece? piece1 = board.GetPieceAtPosition(move1.End.Row, move1.End.Column);
                Piece? piece2 = board.GetPieceAtPosition(move2.End.Row, move2.End.Column);
                int value1 = piece1 != null ? GetPieceValue(piece1) : 0;
                int value2 = piece2 != null ? GetPieceValue(piece2) : 0;
                return value2.CompareTo(value1);  // Capturas más valiosas primero
            });
            return moves;
        }
    }
}



