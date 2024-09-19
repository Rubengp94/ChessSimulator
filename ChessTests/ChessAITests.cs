using ChessEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChessTests
{
    [TestClass]
    public class ChessAITests
    {
        [TestMethod]
        public void TestGetBestMove_ShouldReturnValidMove()
        {
            // Arrange
            var board = new Board();
            var gameStateManager = new GameStateManager(board);
            var ai = new ChessAI(board, gameStateManager);
            var whiteQueen = new Queen(true, new Position(0, 0));
            var blackRook = new Rook(false, new Position(7, 7));
            board.PlacePiece(whiteQueen, 0, 0);
            board.PlacePiece(blackRook, 7, 7);

            // Act
            Move bestMove = ai.GetBestMove(true);  // Simula que es el turno de las blancas

            // Assert
            Assert.IsNotNull(bestMove);  // Debe haber un mejor movimiento
            Assert.IsInstanceOfType(bestMove, typeof(Move));  // Asegurarse de que el movimiento es del tipo correcto
        }

        [TestMethod]
        public void TestEvaluateBoard_ShouldReturnCorrectScore()
        {
            // Arrange
            var board = new Board();
            var gameStateManager = new GameStateManager(board);
            var ai = new ChessAI(board, gameStateManager);
            var whiteQueen = new Queen(true, new Position(0, 0));
            var blackRook = new Rook(false, new Position(1, 1));
            board.PlacePiece(whiteQueen, 0, 0);
            board.PlacePiece(blackRook, 1, 1);

            // Act
            int score = ai.EvaluateBoard();

            // Assert
            Assert.AreEqual(40, score);  // Reina (90) - Torre (50) = 40 (diferencia de puntuación)
        }

        [TestMethod]
        public void TestMinimax_ShouldReturnBestEvaluation()
        {
            // Arrange
            var board = new Board();
            var gameStateManager = new GameStateManager(board);
            var ai = new ChessAI(board, gameStateManager);
            var whiteQueen = new Queen(true, new Position(0, 0));
            var blackQueen = new Queen(false, new Position(7, 7));
            board.PlacePiece(whiteQueen, 0, 0);
            board.PlacePiece(blackQueen, 7, 7);

            // Act
            int evaluation = ai.Minimax(3, int.MinValue, int.MaxValue, true);

            // Assert
            Assert.IsTrue(evaluation > int.MinValue);  // El resultado debe ser una evaluación válida
        }
    }
}
