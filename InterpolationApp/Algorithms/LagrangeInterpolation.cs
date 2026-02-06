using System;
using System.Linq;

namespace InterpolationApp.Algorithms
{
    /// <summary>
    /// Lagrange interpolation - polynomial interpolation using Lagrange basis polynomials
    /// Time complexity: O(n²) for evaluation at single point
    /// Numerical stability: Good for small n, can be unstable for large n due to high-degree polynomials
    /// </summary>
    public class LagrangeInterpolation : IInterpolationAlgorithm
    {
        public string Name => "Lagrange Interpolation";
        public string Description => "Classical polynomial interpolation using Lagrange basis polynomials. " +
                                    "Exact fit through all data points. Best for small datasets (n < 20).";

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

            // Check for duplicate x values
            if (xPoints.Distinct().Count() != xPoints.Length)
                throw new ArgumentException("X values must be unique");

            _xPoints = (double[])xPoints.Clone();
            _yPoints = (double[])yPoints.Clone();
        }

        public double Interpolate(double x)
        {
            if (_xPoints.Length == 0)
                throw new InvalidOperationException("No data points set. Call SetData first.");

            int n = _xPoints.Length;
            double result = 0.0;

            // Calculate Lagrange interpolation: P(x) = Σ y_i * L_i(x)
            // where L_i(x) = Π (x - x_j) / (x_i - x_j) for j ≠ i
            for (int i = 0; i < n; i++)
            {
                double term = _yPoints[i];
                
                // Calculate Lagrange basis polynomial L_i(x)
                for (int j = 0; j < n; j++)
                {
                    if (j != i)
                    {
                        term *= (x - _xPoints[j]) / (_xPoints[i] - _xPoints[j]);
                    }
                }
                
                result += term;
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

            return Math.Sqrt(sumSquaredError / testX.Length); // RMSE
        }
        
        public string GetPolynomialEquation()
        {
            if (_xPoints.Length == 0)
                return "No data points set";
                
            if (_xPoints.Length > 8)
                return $"Polynomial of degree {_xPoints.Length - 1} (too complex to display)";
            
            try
            {
                // Izračunaj koeficijente polinoma ekspanzijom Lagrange baze
                int n = _xPoints.Length;
                double[] coefficients = new double[n];
                
                // Za svaku Lagrange baznu funkciju
                for (int i = 0; i < n; i++)
                {
                    // Izračunaj koeficijente L_i(x)
                    double[] basisCoeffs = new double[n];
                    basisCoeffs[0] = 1.0;
                    int degree = 0;
                    
                    // Množi sa (x - x_j) za sve j != i
                    for (int j = 0; j < n; j++)
                    {
                        if (j != i)
                        {
                            // Pomnoži trenutni polinom sa (x - x_j)
                            double[] newCoeffs = new double[n];
                            for (int k = 0; k <= degree; k++)
                            {
                                newCoeffs[k + 1] += basisCoeffs[k];  // x * koef
                                newCoeffs[k] -= basisCoeffs[k] * _xPoints[j];  // -x_j * koef
                            }
                            basisCoeffs = newCoeffs;
                            degree++;
                        }
                    }
                    
                    // Normalizuj (podijeli sa proizvod (x_i - x_j))
                    double denominator = 1.0;
                    for (int j = 0; j < n; j++)
                    {
                        if (j != i)
                            denominator *= (_xPoints[i] - _xPoints[j]);
                    }
                    
                    // Dodaj y_i * L_i(x) na finalni polinom
                    for (int k = 0; k < n; k++)
                    {
                        coefficients[k] += _yPoints[i] * basisCoeffs[k] / denominator;
                    }
                }
                
                // Formatiraj jednačinu
                return FormatPolynomial(coefficients);
            }
            catch
            {
                return "Unable to generate equation";
            }
        }
        
        private string FormatPolynomial(double[] coefficients)
        {
            var terms = new System.Collections.Generic.List<string>();
            int degree = coefficients.Length - 1;
            
            for (int i = degree; i >= 0; i--)
            {
                double coef = coefficients[i];
                if (Math.Abs(coef) < 1e-10) continue; // Preskoči zanemarljive koeficijente
                
                string term = "";
                double absCoef = Math.Abs(coef);
                
                // Znak
                if (terms.Count > 0)
                    term += coef > 0 ? " + " : " - ";
                else if (coef < 0)
                    term += "-";
                
                // Koeficijent
                if (i == 0 || Math.Abs(absCoef - 1.0) > 1e-10)
                    term += $"{absCoef:F4}";
                
                // Stepen
                if (i > 0)
                {
                    if (Math.Abs(absCoef - 1.0) > 1e-10)
                        term += "·";
                    term += "x";
                    if (i > 1)
                        term += $"^{i}";
                }
                
                terms.Add(term);
            }
            
            return terms.Count > 0 ? $"P(x) = {string.Join("", terms)}" : "P(x) = 0";
        }
    }
}
