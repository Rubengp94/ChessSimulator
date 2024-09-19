using System;

namespace ChessEngine
{
    public class Move
    {
        public Position Start { get; set; }
        public Position End { get; set; }

        public Move(Position start, Position end)
        {
            if (start == null || end == null)
            {
                throw new ArgumentNullException("Las posiciones de inicio y fin no pueden ser nulas.");
            }

            Start = start;
            End = end;
        }

        // Sobrecarga de Equals para comparar movimientos
        public override bool Equals(object? obj)  // Permitir que obj sea nulo
        {
            if (obj is Move otherMove)
            {
                return this.Start.Equals(otherMove.Start) && this.End.Equals(otherMove.End);
            }
            return false;
        }


        // Sobrecarga de GetHashCode para soportar la comparación correcta
        public override int GetHashCode()
        {
            return (Start, End).GetHashCode();
        }
    }
}


