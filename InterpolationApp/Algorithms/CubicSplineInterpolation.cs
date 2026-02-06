using System;
using System.Linq;

namespace InterpolationApp.Algorithms
{
    /// <summary>
    /// Natural Cubic Spline Interpolation
    /// Produces smooth C² continuous curves
    /// Solves tridiagonal system for spline coefficients
    /// Time complexity: O(n) for preprocessing, O(log n) for evaluation
    /// </summary>
    public class CubicSplineInterpolation : IInterpolationAlgorithm
    {
        public string Name => "Cubic Spline (Natural)";
        public string Description => "Piecewise cubic polynomial with C² continuity. " +
                                    "Natural boundary conditions (zero second derivative at endpoints). " +
                                    "Produces very smooth curves, excellent for visualization.";

        private double[] _xPoints = Array.Empty<double>();
        private double[] _yPoints = Array.Empty<double>();
        private double[] _a = Array.Empty<double>();  // Coefficients
        private double[] _b = Array.Empty<double>();
        private double[] _c = Array.Empty<double>();
        private double[] _d = Array.Empty<double>();

        public void SetData(double[] xPoints, double[] yPoints)
        {
            if (xPoints == null || yPoints == null)
                throw new ArgumentNullException("Input arrays cannot be null");

            if (xPoints.Length != yPoints.Length)
                throw new ArgumentException("X and Y arrays must have the same length");

            if (xPoints.Length < 3)
                throw new ArgumentException("At least 3 data points are required for cubic spline");

            if (xPoints.Distinct().Count() != xPoints.Length)
                throw new ArgumentException("X values must be unique");

            // Ensure x points are sorted
            var sorted = xPoints.Zip(yPoints, (x, y) => new { X = x, Y = y })
                                .OrderBy(p => p.X)
                                .ToArray();

            _xPoints = sorted.Select(p => p.X).ToArray();
            _yPoints = sorted.Select(p => p.Y).ToArray();

            CalculateSplineCoefficients();
        }

        private void CalculateSplineCoefficients()
        {
            int n = _xPoints.Length - 1; // number of intervals

            _a = new double[n];
            _b = new double[n];
            _c = new double[n + 1];
            _d = new double[n];

            // h[i] = x[i+1] - x[i]
            double[] h = new double[n];
            for (int i = 0; i < n; i++)
            {
                h[i] = _xPoints[i + 1] - _xPoints[i];
                if (h[i] == 0)
                    throw new ArgumentException("Duplicate x values detected");
            }

            // Set up tridiagonal system for natural spline
            // Natural boundary conditions: c[0] = 0, c[n] = 0
            double[] alpha = new double[n];
            for (int i = 1; i < n; i++)
            {
                alpha[i] = (3.0 / h[i]) * (_yPoints[i + 1] - _yPoints[i]) - 
                           (3.0 / h[i - 1]) * (_yPoints[i] - _yPoints[i - 1]);
            }

            // Solve tridiagonal system using Thomas algorithm
            double[] l = new double[n + 1];
            double[] mu = new double[n + 1];
            double[] z = new double[n + 1];

            l[0] = 1.0;
            mu[0] = 0.0;
            z[0] = 0.0;

            for (int i = 1; i < n; i++)
            {
                l[i] = 2.0 * (_xPoints[i + 1] - _xPoints[i - 1]) - h[i - 1] * mu[i - 1];
                mu[i] = h[i] / l[i];
                z[i] = (alpha[i] - h[i - 1] * z[i - 1]) / l[i];
            }

            l[n] = 1.0;
            z[n] = 0.0;
            _c[n] = 0.0;

            // Back substitution
            for (int j = n - 1; j >= 0; j--)
            {
                _c[j] = z[j] - mu[j] * _c[j + 1];
                _b[j] = (_yPoints[j + 1] - _yPoints[j]) / h[j] - 
                        h[j] * (_c[j + 1] + 2.0 * _c[j]) / 3.0;
                _d[j] = (_c[j + 1] - _c[j]) / (3.0 * h[j]);
                _a[j] = _yPoints[j];
            }
        }

        public double Interpolate(double x)
        {
            if (_xPoints.Length == 0)
                throw new InvalidOperationException("No data points set. Call SetData first.");

            // Find the interval containing x using binary search
            int i = FindInterval(x);

            if (i == -1)
            {
                // Extrapolation - use first interval
                if (x < _xPoints[0])
                    i = 0;
                else
                    i = _xPoints.Length - 2;
            }

            // Calculate spline value: S_i(x) = a_i + b_i(x-x_i) + c_i(x-x_i)² + d_i(x-x_i)³
            double dx = x - _xPoints[i];
            double result = _a[i] + _b[i] * dx + _c[i] * dx * dx + _d[i] * dx * dx * dx;

            return result;
        }

        private int FindInterval(double x)
        {
            // Binary search to find the interval [x[i], x[i+1]] containing x
            if (x < _xPoints[0] || x > _xPoints[_xPoints.Length - 1])
                return -1; // Outside range

            int left = 0;
            int right = _xPoints.Length - 2;

            while (left <= right)
            {
                int mid = (left + right) / 2;

                if (x >= _xPoints[mid] && x <= _xPoints[mid + 1])
                    return mid;
                else if (x < _xPoints[mid])
                    right = mid - 1;
                else
                    left = mid + 1;
            }

            return _xPoints.Length - 2; // Return last interval if not found
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
        /// Calculate first derivative at point x
        /// </summary>
        public double InterpolateDerivative(double x)
        {
            int i = FindInterval(x);
            if (i == -1)
            {
                if (x < _xPoints[0])
                    i = 0;
                else
                    i = _xPoints.Length - 2;
            }

            // S'_i(x) = b_i + 2*c_i(x-x_i) + 3*d_i(x-x_i)²
            double dx = x - _xPoints[i];
            return _b[i] + 2.0 * _c[i] * dx + 3.0 * _d[i] * dx * dx;
        }

        public string GetPolynomialEquation()
        {
            if (_xPoints.Length == 0)
                return "No data points set";

            // Cubic spline is piecewise, complex to show all pieces
            return $"Piecewise cubic spline with {_xPoints.Length - 1} segments (C² continuous, natural boundaries)";
        }
    }
}
