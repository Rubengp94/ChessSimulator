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


        private bool isProcessingMove = false;  // Flag para impedir interacciones mientras se procesa un movimiento

        private void Button_Click(object? sender, EventArgs e)
        {
            if (isProcessingMove)
            {
                Debug.WriteLine("Button_Click: Se está procesando un movimiento, no se permite otra acción.");
                return;
            }

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

            // No hay ninguna pieza seleccionada previamente
            if (selectedPosition == null)
            {
                Debug.WriteLine("Button_Click: No hay ninguna posición seleccionada previamente.");

                if (!isPlayerTurn)
                {
                    Debug.WriteLine("Button_Click: No es el turno del jugador.");
                    return;
                }

                // Obtener la pieza en la posición seleccionada
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
                // Si se hace clic en la misma pieza seleccionada, deseleccionar la pieza
                if (selectedPosition.Row == row && selectedPosition.Column == col)
                {
                    Debug.WriteLine("Button_Click: Deseleccionando la pieza actual.");
                    selectedPosition = null;  // Deseleccionar la pieza
                    ClearHighlightedMoves();   // Limpiar el resaltado
                    return;
                }

                // Intentar mover la pieza seleccionada
                Debug.WriteLine($"Button_Click: Intentando mover de ({selectedPosition.Row}, {selectedPosition.Column}) a ({row}, {col})");

                Position startPosition = new Position(selectedPosition.Row, selectedPosition.Column);
                Position endPosition = new Position(row, col);

                // Validar si las posiciones son válidas en el tablero
                if (!board.IsPositionValid(startPosition) || !board.IsPositionValid(endPosition))
                {
                    Debug.WriteLine("Button_Click: Una de las posiciones es inválida.");
                    return;
                }

                // Evaluar el movimiento
                bool esMovimientoLegalNormal = EvaluarMovimiento(selectedPosition.Row, selectedPosition.Column, row, col);
                Debug.WriteLine($"Button_Click: El movimiento es {(esMovimientoLegalNormal ? "legal" : "ilegal")}.");

                if (esMovimientoLegalNormal)
                {
                    // Establecer la bandera para evitar más acciones durante la actualización gráfica
                    isProcessingMove = true;

                    // Realizar el movimiento y actualizar el gráfico
                    Piece? capturedPiece = board.MovePiece(selectedPosition.Row, selectedPosition.Column, row, col);

                    // Verificar si el peón llega a la fila de promoción (fila 7 para blancas y fila 0 para negras)
                    Piece? movedPiece = board.GetPieceAtPosition(row, col);
                    if (movedPiece is Pawn pawn && (row == 0 || row == 7))
                    {
                        Debug.WriteLine($"Peón en posición de promoción ({row}, {col}). Promocionando a Reina.");
                        pawn.Promote(board, new Position(row, col));
                    }

                    UpdateBoardGraphics();

                    selectedPosition = null;  // Limpiar la selección después de mover

                    // Verificar el estado del juego (jaque, tablas, jaque mate)
                    CheckGameStatus();

                    // Una vez actualizado el gráfico, se pasa el turno a la IA
                    Debug.WriteLine("Button_Click: Turno del jugador terminado, pasando a la IA.");
                    isPlayerTurn = false;
                    MakeAIMove();  // Pasar el turno a la IA

                    // Después de la actualización y el movimiento de la IA, permitir acciones nuevamente
                    isProcessingMove = false;
                }
                else
                {
                    Debug.WriteLine("Button_Click: Movimiento ilegal, intente otra opción.");
                }
            }
        }


        private void MakeAIMove()
        {
            if (!isPlayerTurn)  // Verifica si es el turno de la IA antes de ejecutar cualquier acción
            {
                Debug.WriteLine("Es el turno de la IA.");

                // Obtener el mejor movimiento de la IA
                Move? aiMove = chessAI.GetBestMove(false);
                if (aiMove == null)
                {
                    Debug.WriteLine("Error: No se encontró ningún movimiento válido para la IA.");
                    CheckGameStatus(); // Verificar si la partida terminó
                    return;
                }

                // Realizar el movimiento de la IA
                Debug.WriteLine($"Moviendo {board.GetPieceAtPosition(aiMove.Start.Row, aiMove.Start.Column)?.PieceType} de ({aiMove.Start.Row}, {aiMove.Start.Column}) a ({aiMove.End.Row}, {aiMove.End.Column})");

                // Establecer la bandera para bloquear acciones mientras la IA mueve
                isProcessingMove = true;

                board.MovePiece(aiMove.Start.Row, aiMove.Start.Column, aiMove.End.Row, aiMove.End.Column);

                // Verificar si el peón de la IA llega a la fila de promoción (fila 0 para negras)
                Piece? movedPiece = board.GetPieceAtPosition(aiMove.End.Row, aiMove.End.Column);
                if (movedPiece is Pawn pawn && !pawn.IsWhite && aiMove.End.Row == 0)
                {
                    Debug.WriteLine($"MakeAIMove: Peón negro en fila de promoción ({aiMove.End.Row}, {aiMove.End.Column}). Promocionando a Reina.");
                    pawn.Promote(board, new Position(aiMove.End.Row, aiMove.End.Column));
                }

                UpdateBoardGraphics();

                // Verificar el estado del juego después del movimiento
                CheckGameStatus();

                isPlayerTurn = true;
                Debug.WriteLine("Es el turno del jugador.");

                // Liberar la bandera después de que la IA haya movido
                isProcessingMove = false;
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

                    // Simular el movimiento y verificar si deja al rey en jaque
                    Move movimientoSimulado = new Move(new Position(filaInicio, colInicio), new Position(filaDestino, colDestino));
                    if (gameStateManager.IsMoveInCheck(movimientoSimulado, piezaSeleccionada.IsWhite))
                    {
                        Debug.WriteLine("Movimiento ilegal: dejaría al rey en jaque.");
                        return false;  // Movimiento ilegal ya que deja al rey en jaque
                    }

                    // Mover la pieza
                    board.MovePiece(filaInicio, colInicio, filaDestino, colDestino);
                    Debug.WriteLine($"Movimiento válido: {piezaSeleccionada.PieceType} desde ({filaInicio}, {colInicio}) hasta ({filaDestino}, {colDestino})");

                    // Verificar si el jugador o la IA están en jaque tras el movimiento
                    if (gameStateManager.IsCheck(!piezaSeleccionada.IsWhite))
                    {
                        Debug.WriteLine($"{(piezaSeleccionada.IsWhite ? "Negro" : "Blanco")} está en jaque tras el movimiento.");
                    }

                    // Verificar si se ha producido jaque mate
                    if (gameStateManager.IsCheckMate(!piezaSeleccionada.IsWhite))
                    {
                        Debug.WriteLine($"{(piezaSeleccionada.IsWhite ? "Negro" : "Blanco")} ha recibido jaque mate.");
                    }

                    return true;  // Movimiento válido
                }
            }

            Debug.WriteLine($"Movimiento ilegal: {piezaSeleccionada.PieceType} desde ({filaInicio}, {colInicio}) hasta ({filaDestino}, {colDestino})");
            return false;  // Movimiento ilegal
        }

        private void CheckGameStatus()
        {
            // Si es el turno del jugador, verificamos si la IA está en jaque mate
            if (!isPlayerTurn && gameStateManager.IsCheckMate(false))
            {
                EndGame("¡Has ganado por jaque mate!");  // El jugador ha ganado
                return;
            }

            // Si es el turno de la IA, verificamos si el jugador está en jaque mate
            if (isPlayerTurn && gameStateManager.IsCheckMate(true))
            {
                EndGame("La IA ha ganado por jaque mate.");  // La IA ha ganado
                return;
            }

            // Comprobar si el juego ha terminado en tablas
            if (gameStateManager.IsStalemate(isPlayerTurn))
            {
                EndGame("Fin del juego: Tablas.");
                return;
            }

            // Mensajes de jaque, solo mostrar si el juego sigue
            if (gameStateManager.IsCheck(isPlayerTurn))
            {
                Debug.WriteLine("Estás en jaque.");
            }
            else if (gameStateManager.IsCheck(!isPlayerTurn))
            {
                Debug.WriteLine("La IA está en jaque.");
            }
        }

        private void EndGame(string message)
        {
            // Mostrar mensaje de jaque mate o tablas solo una vez
            MessageBox.Show(message);
            Debug.WriteLine($"Fin del juego: {message}");

            // Crear y mostrar el botón de reinicio
            ShowRestartButton();
        }


        private void ShowRestartButton()
        {
            Button restartButton = new Button
            {
                Text = "Reiniciar Partida",
                Size = new Size(150, 40),
                Location = new Point((ClientSize.Width - 150) / 2, (ClientSize.Height - 40) / 2)  // Centramos el botón
            };

            restartButton.Click += RestartGame;  // Conectar el botón al método de reinicio

            Controls.Add(restartButton);
            restartButton.BringToFront();  // Asegurarse de que el botón esté visible
        }

        private void RestartGame(object? sender, EventArgs e)
        {
            // Limpiar el tablero y reiniciar las piezas
            board = new Board();
            gameStateManager = new GameStateManager(board);
            chessAI = new ChessAI(board, gameStateManager);

            // Limpiar las gráficas y reinicializar
            Controls.Clear();
            InitializeComponent();
            InitializeBoard();
            PlaceInitialPieces();
            UpdateBoardGraphics();

            isPlayerTurn = true;  // Comenzar con el turno del jugador
            selectedPosition = null;  // Limpiar selección
            Debug.WriteLine("Partida reiniciada.");
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
                        button.Image = null;
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

