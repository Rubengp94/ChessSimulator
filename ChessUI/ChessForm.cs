using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ChessEngine;

namespace ChessUI
{
    public partial class ChessForm : Form
    {
        private Board board;
        private ChessAI chessAI;
        private GameStateManager gameStateManager;
        private bool isPlayerTurn = true;
        private Position? selectedPosition = null;

        public ChessForm()
        {
            InitializeComponent();
            board = new Board();
            gameStateManager = new GameStateManager(board);
            chessAI = new ChessAI(board, gameStateManager);
            InitializeBoard();
            PlaceInitialPieces();
            UpdateBoardGraphics();
        }

        private void InitializeComponent()
        {
            this.ClientSize = new Size(480, 480);
            this.Text = "Chess Game";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
        }

        private void InitializeBoard()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Button button = new Button
                    {
                        Width = 60,
                        Height = 60,
                        Location = new Point(col * 60, row * 60),
                        BackColor = (row + col) % 2 == 0 ? Color.White : Color.Gray
                    };
                    button.Click += Button_Click;
                    Controls.Add(button);
                }
            }
        }

        private void PlaceInitialPieces()
        {
            // Colocamos todas las piezas en sus posiciones iniciales usando PlacePiece.
            PlacePiece(new Rook(true, new Position(0, 0)), 0, 0);
            PlacePiece(new Rook(true, new Position(0, 7)), 0, 7);
            PlacePiece(new Rook(false, new Position(7, 0)), 7, 0);
            PlacePiece(new Rook(false, new Position(7, 7)), 7, 7);

            PlacePiece(new Knight(true, new Position(0, 1)), 0, 1);
            PlacePiece(new Knight(true, new Position(0, 6)), 0, 6);
            PlacePiece(new Knight(false, new Position(7, 1)), 7, 1);
            PlacePiece(new Knight(false, new Position(7, 6)), 7, 6);

            PlacePiece(new Bishop(true, new Position(0, 2)), 0, 2);
            PlacePiece(new Bishop(true, new Position(0, 5)), 0, 5);
            PlacePiece(new Bishop(false, new Position(7, 2)), 7, 2);
            PlacePiece(new Bishop(false, new Position(7, 5)), 7, 5);

            PlacePiece(new Queen(true, new Position(0, 3)), 0, 3);
            PlacePiece(new Queen(false, new Position(7, 3)), 7, 3);

            PlacePiece(new King(true, new Position(0, 4)), 0, 4);
            PlacePiece(new King(false, new Position(7, 4)), 7, 4);

            for (int i = 0; i < 8; i++)
            {
                PlacePiece(new Pawn(true, new Position(1, i)), 1, i);
                PlacePiece(new Pawn(false, new Position(6, i)), 6, i);
            }

            Console.WriteLine($"Rey blanco en (0, 4), Rey negro en (7, 4)");
        }

        private void PlacePiece(Piece piece, int row, int col)
        {
            if (board.GetPieceAtPosition(row, col) != null)
            {
                Debug.WriteLine($"Error: Ya hay una pieza en la posición ({row}, {col})");
            }

            board.PlacePiece(piece, row, col);  // Coloca la pieza en el tablero lógico

            Button button = GetButtonAtPosition(new Position(row, col));
            string imageFileName = $"{piece.PieceType}_{(piece.IsWhite ? "White" : "Black")}.png";
            string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", imageFileName);

            // Actualizar el gráfico en el botón con la imagen correspondiente de la pieza
            if (System.IO.File.Exists(imagePath))
            {
                button.Image = Image.FromFile(imagePath);
            }
            else
            {
                MessageBox.Show($"La imagen {imageFileName} no se encuentra en la carpeta Resources.");
            }
        }


        private void Button_Click(object? sender, EventArgs e)
        {
            Debug.WriteLine("Button_Click: Se ha hecho clic en un botón.");

            Button? clickedButton = sender as Button;
            if (clickedButton == null)
            {
                Debug.WriteLine("Button_Click: El botón clickeado es nulo.");
                return;
            }

            int row = clickedButton.Location.Y / 60;
            int col = clickedButton.Location.X / 60;

            Debug.WriteLine($"Button_Click: Coordenadas del botón clickeado - Fila: {row}, Columna: {col}");

            if (selectedPosition == null)
            {
                Debug.WriteLine("Button_Click: No hay ninguna posición seleccionada previamente.");

                if (!isPlayerTurn)
                {
                    Debug.WriteLine("Button_Click: No es el turno del jugador.");
                    return;
                }

                Piece? piece = board.GetPieceAtPosition(row, col);
                if (piece != null && piece.IsWhite == isPlayerTurn)
                {
                    selectedPosition = new Position(row, col);
                    Debug.WriteLine($"Button_Click: Pieza seleccionada: {piece.PieceType} en ({row}, {col})");
                    HighlightSelectedPiece(selectedPosition);
                }
                else
                {
                    Debug.WriteLine("Button_Click: No se seleccionó ninguna pieza válida.");
                }
            }
            else
            {
                Debug.WriteLine($"Button_Click: Intentando mover de ({selectedPosition.Row}, {selectedPosition.Column}) a ({row}, {col})");

                Position startPosition = new Position(selectedPosition.Row, selectedPosition.Column);
                Position endPosition = new Position(row, col);

                if (!board.IsPositionValid(startPosition) || !board.IsPositionValid(endPosition))
                {
                    Debug.WriteLine("Button_Click: Una de las posiciones es inválida.");
                    return;
                }

                bool esMovimientoLegal = EvaluarMovimiento(selectedPosition.Row, selectedPosition.Column, row, col);
                Debug.WriteLine($"Button_Click: El movimiento es {(esMovimientoLegal ? "legal" : "ilegal")}.");

                if (esMovimientoLegal)
                {
                    Piece? capturedPiece = board.MovePiece(selectedPosition.Row, selectedPosition.Column, row, col);

                    // Solo actualizar gráficos después de que el movimiento esté completo y verificado
                    UpdateBoardGraphics();

                    selectedPosition = null;
                    CheckGameStatus();

                    Debug.WriteLine("Button_Click: Turno del jugador terminado, pasando a la IA.");
                    isPlayerTurn = false;
                    MakeAIMove();  // Pasar el turno a la IA
                }
                else
                {
                    Debug.WriteLine("Button_Click: Movimiento ilegal, intente otra opción.");
                }
            }
        }




        private bool EvaluarMovimiento(int filaInicio, int colInicio, int filaDestino, int colDestino)
        {
            // Obtener la pieza en la posición inicial
            Piece? piezaSeleccionada = board.GetPieceAtPosition(filaInicio, colInicio);

            if (piezaSeleccionada == null)
            {
                Debug.WriteLine("No hay ninguna pieza en la posición inicial.");
                return false;  // Movimiento ilegal
            }

            // Verificar si la posición de destino tiene una pieza del mismo color
            Piece? piezaDestino = board.GetPieceAtPosition(filaDestino, colDestino);
            if (piezaDestino != null && piezaDestino.IsWhite == piezaSeleccionada.IsWhite)
            {
                Debug.WriteLine("Movimiento ilegal: no puedes mover una pieza sobre otra de tu mismo color.");
                return false;
            }

            // Obtener los movimientos válidos de la pieza seleccionada
            List<Move> movimientosValidos = piezaSeleccionada.GetValidMoves(board);

            // Verificar si el movimiento deseado está en la lista de movimientos válidos
            foreach (var move in movimientosValidos)
            {
                if (move.End.Row == filaDestino && move.End.Column == colDestino)
                {
                    // Verificar si el movimiento es un enroque
                    if (piezaSeleccionada is King && Math.Abs(colInicio - colDestino) == 2)
                    {
                        // Movimiento de enroque: mover también la torre
                        if (colDestino == 6) // Enroque corto
                        {
                            board.MovePiece(filaInicio, 7, filaInicio, 5);  // Mover la torre
                        }
                        else if (colDestino == 2) // Enroque largo
                        {
                            board.MovePiece(filaInicio, 0, filaInicio, 3);  // Mover la torre
                        }
                    }

                    // Mover la pieza
                    board.MovePiece(filaInicio, colInicio, filaDestino, colDestino);
                    Debug.WriteLine($"Movimiento válido: {piezaSeleccionada.PieceType} desde ({filaInicio}, {colInicio}) hasta ({filaDestino}, {colDestino})");

                    return true;  // Movimiento válido
                }
            }

            Debug.WriteLine($"Movimiento ilegal: {piezaSeleccionada.PieceType} desde ({filaInicio}, {colInicio}) hasta ({filaDestino}, {colDestino})");
            return false;  // Movimiento ilegal
        }



        private void MakeAIMove()
        {
            if (!isPlayerTurn)  // Verifica si es el turno de la IA antes de ejecutar cualquier acción
            {
                Debug.WriteLine("Es el turno de la IA.");

                // Verificar si el rey sigue en el tablero antes del movimiento de la IA
                Position whiteKingPosition = gameStateManager.GetKingPosition(true);
                Position blackKingPosition = gameStateManager.GetKingPosition(false);
                Debug.WriteLine($"Rey blanco en: ({whiteKingPosition.Row}, {whiteKingPosition.Column}), Rey negro en: ({blackKingPosition.Row}, {blackKingPosition.Column})");

                Move? aiMove = chessAI.GetBestMove(false);
                if (aiMove != null)
                {
                    Debug.WriteLine($"Moviendo {board.GetPieceAtPosition(aiMove.Start.Row, aiMove.Start.Column)?.PieceType} de ({aiMove.Start.Row}, {aiMove.Start.Column}) a ({aiMove.End.Row}, {aiMove.End.Column})");

                    board.MovePiece(aiMove.Start.Row, aiMove.Start.Column, aiMove.End.Row, aiMove.End.Column);
                    UpdateBoardGraphics();

                    CheckGameStatus(); // Verifica el estado del juego después de que la IA realiza un movimiento
                }

                // Cambiar de turno nuevamente para que el jugador pueda jugar
                isPlayerTurn = true;
                Debug.WriteLine("Es el turno del jugador.");
            }
        }

        private void CheckGameStatus()
        {
            if (gameStateManager.IsCheckMate(isPlayerTurn))
            {
                MessageBox.Show(isPlayerTurn ? "¡Has ganado por jaque mate!" : "La IA ha ganado por jaque mate");
                Debug.WriteLine("Jaque mate.");
            }
            else if (gameStateManager.IsStalemate(isPlayerTurn))
            {
                MessageBox.Show("El juego ha terminado en tablas");
                Debug.WriteLine("Tablas.");
            }
            else if (gameStateManager.IsCheck(isPlayerTurn))
            {
                MessageBox.Show(isPlayerTurn ? "Estás en jaque" : "La IA está en jaque");
                Debug.WriteLine("Jaque.");
            }
        }

        private void UpdateBoardGraphics()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Button button = GetButtonAtPosition(new Position(row, col));
                    Piece? piece = board.GetPieceAtPosition(row, col);

                    if (piece != null)
                    {
                        string imageFileName = $"{piece.PieceType}_{(piece.IsWhite ? "White" : "Black")}.png";
                        if (System.IO.File.Exists($"Resources/{imageFileName}"))
                        {
                            Debug.WriteLine($"Actualizando gráfico para {piece.PieceType} en ({row}, {col})");
                            button.Image = Image.FromFile($"Resources/{imageFileName}");
                        }
                        else
                        {
                            Debug.WriteLine($"Imagen no encontrada para {piece.PieceType} en ({row}, {col})");
                            button.Image = null;
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Limpiando gráfico en ({row}, {col})");
                        button.Image = null;  // Si no hay pieza, eliminar la imagen previa
                    }
                }
            }
        }


        private void HighlightSelectedPiece(Position position)
        {
            ClearHighlightedMoves();  // Limpiar resaltado anterior
            Button? button = GetButtonAtPosition(position);
            if (button != null)
            {
                button.BackColor = Color.LightBlue;  // Resaltar la pieza seleccionada
            }
        }

        private void ClearHighlightedMoves()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Button button = GetButtonAtPosition(new Position(row, col));
                    if (button != null)
                    {
                        button.BackColor = (row + col) % 2 == 0 ? Color.White : Color.Gray;  // Restaurar colores del tablero
                    }
                }
            }
        }

        private Button? GetButtonAtPosition(Position position)
        {
            foreach (Control control in Controls)
            {
                if (control is Button button)
                {
                    int row = button.Location.Y / 60;
                    int col = button.Location.X / 60;
                    if (row == position.Row && col == position.Column)
                    {
                        return button;  // Retorna el botón en la posición dada
                    }
                }
            }
            return null;
        }
    }
}

