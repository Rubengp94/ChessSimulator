using ChessEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChessTests
{
    [TestClass]
    public class BoardTests
    {
        [TestMethod]
        public void TestPlacePiece_OnValidPosition_ShouldPlacePiece()
        {
            // Arrange
            var board = new Board();
            var piece = new Queen(true, new Position(0, 0));  // Usa una clase derivada concreta

            // Act
            board.PlacePiece(piece, 0, 0);

            // Assert
            var placedPiece = board.GetPieceAtPosition(0, 0);  // Obtener la referencia de la pieza
            Assert.IsNotNull(placedPiece);  // Comprobar que la posición no está vacía
            Assert.AreEqual(piece, placedPiece);  // Verificar que la pieza es la esperada
        }

        [TestMethod]
        public void TestMovePiece_ShouldMovePieceToNewPosition()
        {
            // Arrange
            var board = new Board();
            var piece = new Queen(true, new Position(0, 0));  // Usa una clase derivada concreta
            board.PlacePiece(piece, 0, 0);

            // Act
            board.MovePiece(0, 0, 1, 1);

            // Assert
            var startPiece = board.GetPieceAtPosition(0, 0);
            var endPiece = board.GetPieceAtPosition(1, 1);
            Assert.IsNull(startPiece);  // La posición inicial debe estar vacía
            Assert.IsNotNull(endPiece);  // La nueva posición debe tener una pieza
            Assert.AreEqual(piece, endPiece);  // Verificar que la pieza es la misma
        }

        [TestMethod]
        public void TestIsPositionOccupied_ReturnsTrueIfOccupied()
        {
            // Arrange
            var board = new Board();
            var piece = new Queen(true, new Position(0, 0));
            board.PlacePiece(piece, 0, 0);

            // Act
            bool isOccupied = board.IsPositionOccupied(new Position(0, 0));

            // Assert
            Assert.IsTrue(isOccupied);  // La posición debe estar ocupada
        }

        [TestMethod]
        public void TestIsPositionOccupiedByEnemy_ReturnsTrueForEnemyPiece()
        {
            // Arrange
            var board = new Board();
            var whitePiece = new Queen(true, new Position(0, 0));
            var blackPiece = new Queen(false, new Position(1, 1));
            board.PlacePiece(whitePiece, 0, 0);
            board.PlacePiece(blackPiece, 1, 1);

            // Act
            bool isOccupiedByEnemy = board.IsPositionOccupiedByEnemyPiece(new Position(1, 1), true);

            // Assert
            Assert.IsTrue(isOccupiedByEnemy);  // Debe retornar true si la pieza es enemiga
        }
    }
}
