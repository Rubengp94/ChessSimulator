using System;

namespace ChessEngine
{
    public class Position
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }

        // Sobrecarga de Equals para comparar posiciones
        public override bool Equals(object obj)
        {
            if (obj is Position other)
            {
                return this.Row == other.Row && this.Column == other.Column;
            }
            return false;
        }

        // Sobrecarga de GetHashCode para soportar la comparación correcta
        public override int GetHashCode()
        {
            return (Row, Column).GetHashCode();
        }
    }
}
