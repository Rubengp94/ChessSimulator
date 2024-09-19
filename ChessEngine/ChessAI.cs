using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ChessEngine
{
    public class ChessAI
    {
        private const int MaxDepth = 1;
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
                Piece capturedPiece = board.MovePiece(move.Start.Row, move.Start.Column, move.End.Row, move.End.Column);

                // Evaluar el movimiento usando Minimax
                int moveScore = Minimax(MaxDepth, int.MinValue, int.MaxValue, !isWhite);

                // Deshacer el movimiento
                board.MovePiece(move.End.Row, move.End.Column, move.Start.Row, move.Start.Column);

                // Restaurar la pieza capturada
                if (capturedPiece != null)
                {
                    board.PlacePiece(capturedPiece, move.End.Row, move.End.Column);
                }

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

        internal int Minimax(int depth, int alpha, int beta, bool isMaximizingPlayer)
        {
            if (depth == 0 || gameStateManager.IsGameOver())
            {
                return EvaluateBoard();
            }

            List<Move> moves = GetAllValidMoves(isMaximizingPlayer);

            if (isMaximizingPlayer)
            {
                int maxEval = int.MinValue;
                foreach (var move in moves)
                {
                    Piece piece = board.GetPieceAtPosition(move.Start.Row, move.Start.Column);
                    if (piece == null) continue;

                    // Mover la pieza temporalmente
                    Piece capturedPiece = board.MovePiece(move.Start.Row, move.Start.Column, move.End.Row, move.End.Column);

                    int eval = Minimax(depth - 1, alpha, beta, false);

                    // Deshacer el movimiento
                    board.MovePiece(move.End.Row, move.End.Column, move.Start.Row, move.Start.Column);

                    // Restaurar la pieza capturada
                    if (capturedPiece != null)
                    {
                        board.PlacePiece(capturedPiece, move.End.Row, move.End.Column);
                    }

                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha) break;
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
                    Piece capturedPiece = board.MovePiece(move.Start.Row, move.Start.Column, move.End.Row, move.End.Column);

                    int eval = Minimax(depth - 1, alpha, beta, true);

                    // Deshacer el movimiento
                    board.MovePiece(move.End.Row, move.End.Column, move.Start.Row, move.Start.Column);

                    // Restaurar la pieza capturada
                    if (capturedPiece != null)
                    {
                        board.PlacePiece(capturedPiece, move.End.Row, move.End.Column);
                    }

                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha) break;
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
                    Piece piece = board.GetPieceAtPosition(row, col);
                    if (piece != null)
                    {
                        score += GetPieceValue(piece);
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
                    Piece piece = board.GetPieceAtPosition(row, col);
                    if (piece != null && piece.IsWhite == isWhite)
                    {
                        Debug.WriteLine($"Generando movimientos para {piece.PieceType} en ({row}, {col})");
                        List<Move> validMoves = piece.GetValidMoves(board);
                        moves.AddRange(validMoves);
                    }
                }
            }
            Debug.WriteLine($"Total de movimientos válidos generados para {(isWhite ? "blancas" : "negras")}: {moves.Count}");
            return moves;
        }
    }
}
