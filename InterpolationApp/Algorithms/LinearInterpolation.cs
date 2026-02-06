using System;
using System.Linq;

namespace InterpolationApp.Algorithms
{
    /// <summary>
    /// Linear (Piecewise Linear) Interpolation
    /// Simplest interpolation method - connects points with straight lines
    /// Very fast and numerically stable
    /// </summary>
    public class LinearInterpolation : IInterpolationAlgorithm
    {
        public string Name => "Linear Interpolation";
        public string Description => "Simplest method - connects points with straight lines. " +
                                    "Very fast, numerically stable, but not smooth (C⁰ continuous).";

        private double[] _xPoints = Array.Empty<double>();
        private double[] _yPoints = Array.Empty<double>();

        public void SetData(double[] xPoints, double[] yPoints)
        {
            if (xPoints == null || yPoints == null)
                throw new ArgumentNullException("Input arrays cannot be null");

            if (xPoints.Length != yPoints.Length)
                throw new ArgumentException("X and Y arrays must have the same length");

            if (xPoints.Length < 2)
                throw new ArgumentException("At least 2 data points are required");

            if (xPoints.Distinct().Count() != xPoints.Length)
                throw new ArgumentException("X values must be unique");

            // Sort data points
            var sorted = xPoints.Zip(yPoints, (x, y) => new { X = x, Y = y })
                                .OrderBy(p => p.X)
                                .ToArray();

            _xPoints = sorted.Select(p => p.X).ToArray();
            _yPoints = sorted.Select(p => p.Y).ToArray();
        }

        public double Interpolate(double x)
        {
            if (_xPoints.Length == 0)
                throw new InvalidOperationException("No data points set. Call SetData first.");

            // Handle extrapolation
            if (x <= _xPoints[0])
            {
                // Linear extrapolation using first two points
                if (_xPoints.Length == 1)
                    return _yPoints[0];

                double slope = (_yPoints[1] - _yPoints[0]) / (_xPoints[1] - _xPoints[0]);
                return _yPoints[0] + slope * (x - _xPoints[0]);
            }

            if (x >= _xPoints[_xPoints.Length - 1])
            {
                // Linear extrapolation using last two points
                int n = _xPoints.Length;
                if (n == 1)
                    return _yPoints[0];

                double slope = (_yPoints[n - 1] - _yPoints[n - 2]) / (_xPoints[n - 1] - _xPoints[n - 2]);
                return _yPoints[n - 1] + slope * (x - _xPoints[n - 1]);
            }

            // Find the interval containing x
            int i = 0;
            for (i = 0; i < _xPoints.Length - 1; i++)
            {
                if (x >= _xPoints[i] && x <= _xPoints[i + 1])
                    break;
            }

            // Linear interpolation: y = y0 + (y1 - y0) * (x - x0) / (x1 - x0)
            double x0 = _xPoints[i];
            double x1 = _xPoints[i + 1];
            double y0 = _yPoints[i];
            double y1 = _yPoints[i + 1];

            double t = (x - x0) / (x1 - x0);
            return y0 + (y1 - y0) * t;
        }

        public double[] InterpolateRange(double xMin, double xMax, int numPoints)
        {
            if (numPoints < 2)
                throw new ArgumentException("Number of points must be at least 2");

            double[] results = new double[numPoints];
            double step = (xMax - xMin) / (numPoints - 1);

            for (int i = 0; i < numPoints; i++)
            {
                double x = xMin + i * step;
                results[i] = Interpolate(x);
            }

            return results;
        }

        public double CalculateError(double[] testX, double[] testY)
        {
            if (testX.Length != testY.Length)
                throw new ArgumentException("Test arrays must have the same length");

            double sumSquaredError = 0.0;

            for (int i = 0; i < testX.Length; i++)
            {
                double interpolated = Interpolate(testX[i]);
                double error = interpolated - testY[i];
                sumSquaredError += error * error;
            }

            return Math.Sqrt(sumSquaredError / testX.Length);
        }

        public string GetPolynomialEquation()
        {
            if (_xPoints.Length == 0)
                return "No data points set";

            // Linear interpolation is piecewise, so we return a description
            return $"Piecewise linear with {_xPoints.Length} segments";
        }
    }
}
