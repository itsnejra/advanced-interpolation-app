using System;

namespace InterpolationApp.Models
{
    /// <summary>
    /// Represents a single data point (x, y)
    /// </summary>
    public class DataPoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public DataPoint()
        {
        }

        public DataPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"({X:F4}, {Y:F4})";
        }

        public override bool Equals(object? obj)
        {
            if (obj is DataPoint other)
            {
                return Math.Abs(X - other.X) < 1e-10 && Math.Abs(Y - other.Y) < 1e-10;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}
