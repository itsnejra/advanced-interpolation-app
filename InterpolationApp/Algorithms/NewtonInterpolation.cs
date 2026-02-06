using System;
using System.Linq;

namespace InterpolationApp.Algorithms
{
    /// <summary>
    /// Newton interpolation using divided differences
    /// Time complexity: O(n²) for preprocessing, O(n) for evaluation
    /// Numerically more stable than Lagrange for repeated evaluations
    /// </summary>
    public class NewtonInterpolation : IInterpolationAlgorithm
    {
        public string Name => "Newton Divided Differences";
        public string Description => "Polynomial interpolation using Newton's divided difference formula. " +
                                    "More efficient than Lagrange for multiple evaluations. Good numerical stability.";

        private double[] _xPoints = Array.Empty<double>();
        private double[] _yPoints = Array.Empty<double>();
        private double[] _dividedDifferences = Array.Empty<double>();

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

            _xPoints = (double[])xPoints.Clone();
            _yPoints = (double[])yPoints.Clone();

            // Calculate divided differences
            CalculateDividedDifferences();
        }

        private void CalculateDividedDifferences()
        {
            int n = _xPoints.Length;
            _dividedDifferences = new double[n];

            // Create a table for divided differences
            double[,] table = new double[n, n];

            // First column is just the y values
            for (int i = 0; i < n; i++)
            {
                table[i, 0] = _yPoints[i];
            }

            // Calculate divided differences
            // f[x_i, x_{i+1}, ..., x_{i+k}] = (f[x_{i+1}, ..., x_{i+k}] - f[x_i, ..., x_{i+k-1}]) / (x_{i+k} - x_i)
            for (int j = 1; j < n; j++)
            {
                for (int i = 0; i < n - j; i++)
                {
                    table[i, j] = (table[i + 1, j - 1] - table[i, j - 1]) / (_xPoints[i + j] - _xPoints[i]);
                }
            }

            // Store the diagonal (these are the coefficients we need)
            for (int i = 0; i < n; i++)
            {
                _dividedDifferences[i] = table[0, i];
            }
        }

        public double Interpolate(double x)
        {
            if (_xPoints.Length == 0)
                throw new InvalidOperationException("No data points set. Call SetData first.");

            int n = _xPoints.Length;
            double result = _dividedDifferences[0];
            double term = 1.0;

            // P(x) = f[x_0] + f[x_0,x_1](x-x_0) + f[x_0,x_1,x_2](x-x_0)(x-x_1) + ...
            for (int i = 1; i < n; i++)
            {
                term *= (x - _xPoints[i - 1]);
                result += _dividedDifferences[i] * term;
            }

            return result;
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
        /// Get the divided differences table for analysis
        /// </summary>
        public double[] GetDividedDifferences()
        {
            return (double[])_dividedDifferences.Clone();
        }

        public string GetPolynomialEquation()
        {
            if (_xPoints.Length == 0)
                return "No data points set";

            int n = _xPoints.Length;
            var terms = new System.Collections.Generic.List<string>();

            // First term is just the first divided difference
            terms.Add($"{_dividedDifferences[0]:F4}");

            // Build Newton's form: f[x0] + f[x0,x1](x-x0) + f[x0,x1,x2](x-x0)(x-x1) + ...
            for (int i = 1; i < n; i++)
            {
                string term = $"{_dividedDifferences[i]:F4}";
                for (int j = 0; j < i; j++)
                {
                    term += $"(x-{_xPoints[j]:F2})";
                }
                terms.Add(term);
            }

            return "P(x) = " + string.Join(" + ", terms);
        }
    }
}
