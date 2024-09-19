using System.Collections.Generic;

namespace ChessEngine
{
    public static class MasterTables
    {
        // Diccionario que almacena los movimientos posibles para piezas con movimientos predefinidos
        public static readonly Dictionary<string, List<(int, int)>> PieceMovementTable = new Dictionary<string, List<(int, int)>>
        {
            // Movimientos del Caballo
            { "Knight", new List<(int, int)>
                {
                    (-2, -1), (-2, 1), (2, -1), (2, 1),
                    (-1, -2), (-1, 2), (1, -2), (1, 2)
                }
            },

            // Movimientos del Rey (una casilla en cualquier dirección)
            { "King", new List<(int, int)>
                {
                    (1, 1),  (1, 0),  (1, -1),   // Movimientos hacia abajo (fila +1)
                    (0, 1),          (0, -1),    // Movimientos hacia los lados
                    (-1, 1), (-1, 0), (-1, -1)   // Movimientos hacia arriba (fila -1)
                }
            },

            // Movimientos de la Reina (combina los de la Torre y el Alfil)
            { "Queen", new List<(int, int)>
                {
                    // Movimientos en línea recta (Torre)
                    (1, 0), (-1, 0), (0, 1), (0, -1),
                    // Movimientos diagonales (Alfil)
                    (1, 1), (1, -1), (-1, 1), (-1, -1)
                }
            },

            // Movimientos de la Torre (líneas rectas)
            { "Rook", new List<(int, int)>
                {
                    (1, 0), (-1, 0), (0, 1), (0, -1)
                }
            },

            // Movimientos del Alfil (diagonales)
            { "Bishop", new List<(int, int)>
                {
                    (1, 1), (1, -1), (-1, 1), (-1, -1)
                }
            },

            // Movimientos estándar del Peón
            { "PawnWhite", new List<(int, int)>
                {
                    (1, 0)  // Solo hacia adelante para los peones blancos (hacia arriba)
                }
            },
            { "PawnBlack", new List<(int, int)>
                {
                    (-1, 0)  // Solo hacia adelante para los peones negros (hacia abajo)
                }
            },
        };

        // Tabla para los movimientos diagonales de los peones (captura)
        public static readonly Dictionary<string, List<(int, int)>> PawnCaptureMoves = new Dictionary<string, List<(int, int)>>
        {
            { "PawnWhite", new List<(int, int)>
                {
                    (-1, 1), (-1, -1)  // Movimiento diagonal hacia adelante para capturar
                }
            },
            { "PawnBlack", new List<(int, int)>
                {
                    (1, 1), (1, -1)  // Movimiento diagonal hacia adelante para capturar (peones negros)
                }
            }
        };

        // Tabla para los movimientos iniciales de los peones (dos casillas adelante)
        public static readonly Dictionary<string, List<(int, int)>> InitialPawnMoves = new Dictionary<string, List<(int, int)>>
        {
            { "PawnWhite", new List<(int, int)>
                {
                    (-2, 0)  // Mover dos casillas hacia adelante en el primer movimiento
                }
            },
            { "PawnBlack", new List<(int, int)>
                {
                    (2, 0)  // Mover dos casillas hacia adelante en el primer movimiento
                }
            }
        };

        // Movimientos especiales de piezas que no se pueden definir con un patrón continuo
        public static readonly Dictionary<string, string> SpecialMoves = new Dictionary<string, string>
        {
            { "Castling", "Special move for King and Rook" },   // Enroque
            { "EnPassant", "Special move for Pawn" }            // Captura al paso
        };
    }
}
