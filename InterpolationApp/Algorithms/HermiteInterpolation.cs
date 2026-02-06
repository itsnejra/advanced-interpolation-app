using System;
using System.Linq;

namespace InterpolationApp.Algorithms
{
    /// <summary>
    /// Hermite Interpolation - uses both function values and derivatives
    /// Produces C¹ continuous curves
    /// Useful when derivative information is available
    /// </summary>
    public class HermiteInterpolation : IInterpolationAlgorithm
    {
        public string Name => "Hermite Interpolation";
        public string Description => "Piecewise cubic interpolation using function values AND derivatives. " +
                                    "Produces C¹ continuous curves. Derivatives estimated using finite differences.";

        private double[] _xPoints = Array.Empty<double>();
        private double[] _yPoints = Array.Empty<double>();
        private double[] _derivatives = Array.Empty<double>();

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

            // Estimate derivatives using finite differences
            EstimateDerivatives();
        }

        /// <summary>
        /// Set data with explicit derivatives
        /// </summary>
        public void SetDataWithDerivatives(double[] xPoints, double[] yPoints, double[] derivatives)
        {
            if (xPoints == null || yPoints == null || derivatives == null)
                throw new ArgumentNullException("Input arrays cannot be null");

            if (xPoints.Length != yPoints.Length || xPoints.Length != derivatives.Length)
                throw new ArgumentException("All arrays must have the same length");

            if (xPoints.Length < 2)
                throw new ArgumentException("At least 2 data points are required");

            _xPoints = (double[])xPoints.Clone();
            _yPoints = (double[])yPoints.Clone();
            _derivatives = (double[])derivatives.Clone();
        }

        private void EstimateDerivatives()
        {
            int n = _xPoints.Length;
            _derivatives = new double[n];

            // Use central differences for interior points
            for (int i = 1; i < n - 1; i++)
            {
                // f'(x_i) ≈ (f(x_{i+1}) - f(x_{i-1})) / (x_{i+1} - x_{i-1})
                _derivatives[i] = (_yPoints[i + 1] - _yPoints[i - 1]) / 
                                 (_xPoints[i + 1] - _xPoints[i - 1]);
            }

            // Forward difference for first point
            _derivatives[0] = (_yPoints[1] - _yPoints[0]) / (_xPoints[1] - _xPoints[0]);

            // Backward difference for last point
            _derivatives[n - 1] = (_yPoints[n - 1] - _yPoints[n - 2]) / 
                                  (_xPoints[n - 1] - _xPoints[n - 2]);
        }

        public double Interpolate(double x)
        {
            if (_xPoints.Length == 0)
                throw new InvalidOperationException("No data points set. Call SetData first.");

            // Find the interval containing x
            int i = FindInterval(x);

            if (i == -1)
            {
                // Extrapolation
                if (x < _xPoints[0])
                {
                    i = 0;
                }
                else
                {
                    i = _xPoints.Length - 2;
                }
            }

            // Hermite interpolation on interval [x_i, x_{i+1}]
            double x0 = _xPoints[i];
            double x1 = _xPoints[i + 1];
            double y0 = _yPoints[i];
            double y1 = _yPoints[i + 1];
            double m0 = _derivatives[i];
            double m1 = _derivatives[i + 1];

            // Normalize to [0, 1]
            double h = x1 - x0;
            double t = (x - x0) / h;

            // Hermite basis functions
            double h00 = (1 + 2 * t) * (1 - t) * (1 - t);  // (1 + 2t)(1-t)²
            double h10 = t * (1 - t) * (1 - t);             // t(1-t)²
            double h01 = t * t * (3 - 2 * t);               // t²(3-2t)
            double h11 = t * t * (t - 1);                   // t²(t-1)

            // Hermite polynomial: H(t) = y0*h00 + h*m0*h10 + y1*h01 + h*m1*h11
            double result = y0 * h00 + h * m0 * h10 + y1 * h01 + h * m1 * h11;

            return result;
        }

        private int FindInterval(double x)
        {
            if (x < _xPoints[0] || x > _xPoints[_xPoints.Length - 1])
                return -1;

            for (int i = 0; i < _xPoints.Length - 1; i++)
            {
                if (x >= _xPoints[i] && x <= _xPoints[i + 1])
                    return i;
            }

            return _xPoints.Length - 2;
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

        /// <summary>
        /// Get the estimated derivatives for analysis
        /// </summary>
        public double[] GetDerivatives()
        {
            return (double[])_derivatives.Clone();
        }

        public string GetPolynomialEquation()
        {
            if (_xPoints.Length == 0)
                return "No data points set";

            // Hermite interpolation is piecewise cubic, complex to show all pieces
            return $"Piecewise cubic Hermite with {_xPoints.Length - 1} segments (C¹ continuous)";
        }
    }
}
