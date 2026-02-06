using System;
using System.Collections.Generic;
using InterpolationApp.Algorithms;

namespace InterpolationApp.Helpers
{
    /// <summary>
    /// Finds optimal polynomial degree for desired interpolation accuracy
    /// </summary>
    public class PolynomialDegreeOptimizer
    {
        private readonly FunctionEvaluatorFixed _function;
        private readonly double _xMin;
        private readonly double _xMax;
        private readonly string _algorithmName;

        public PolynomialDegreeOptimizer(FunctionEvaluatorFixed function, double xMin, double xMax, string algorithmName)
        {
            _function = function ?? throw new ArgumentNullException(nameof(function));
            _xMin = xMin;
            _xMax = xMax;
            _algorithmName = algorithmName ?? "Cubic Spline";
        }

        /// <summary>
        /// Find minimum polynomial degree n that achieves maxError <= targetError
        /// </summary>
        public (int minDegree, double actualError, List<(int n, double error)> results) FindMinimumDegree(
            double targetError, 
            int minN = 2, 
            int maxN = 50)
        {
            var results = new List<(int n, double error)>();

            for (int n = minN; n <= maxN; n++)
            {
                try
                {
                    // Generate samples
                    var (x, y) = _function.GenerateChebyshevSamples(_xMin, _xMax, n);

                    // Create interpolator
                    var algorithm = CreateAlgorithm(_algorithmName);
                    algorithm.SetData(x, y);

                    // Calculate error
                    double error = _function.CalculateMaxError(algorithm, _xMin, _xMax, 1000);
                    results.Add((n, error));

                    // Check if target achieved
                    if (error <= targetError)
                    {
                        return (n, error, results);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[OPTIMIZER] n={n} failed: {ex.Message}");
                }
            }

            // Target not achieved
            var lastResult = results[results.Count - 1];
            return (lastResult.n, lastResult.error, results);
        }

        /// <summary>
        /// Test multiple polynomial degrees and return error analysis
        /// </summary>
        public List<(int n, double maxError, double avgError)> AnalyzeDegreeRange(int minN, int maxN, int testPoints = 1000)
        {
            var results = new List<(int n, double maxError, double avgError)>();

            for (int n = minN; n <= maxN; n++)
            {
                try
                {
                    var (x, y) = _function.GenerateChebyshevSamples(_xMin, _xMax, n);
                    var algorithm = CreateAlgorithm(_algorithmName);
                    algorithm.SetData(x, y);

                    double maxError = 0;
                    double sumError = 0;
                    double step = (_xMax - _xMin) / (testPoints - 1);

                    for (int i = 0; i < testPoints; i++)
                    {
                        double xTest = _xMin + i * step;
                        double actual = _function.Evaluate(xTest);
                        double interpolated = algorithm.Interpolate(xTest);
                        double error = Math.Abs(actual - interpolated);
                        
                        maxError = Math.Max(maxError, error);
                        sumError += error;
                    }

                    double avgError = sumError / testPoints;
                    results.Add((n, maxError, avgError));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ANALYZER] n={n} failed: {ex.Message}");
                }
            }

            return results;
        }

        private IInterpolationAlgorithm CreateAlgorithm(string name)
        {
            return name switch
            {
                "Lagrange" => new LagrangeInterpolation(),
                "Newton" => new NewtonInterpolation(),
                "Cubic Spline" => new CubicSplineInterpolation(),
                "Hermite" => new HermiteInterpolation(),
                "Linear" => new LinearInterpolation(),
                _ => new CubicSplineInterpolation()
            };
        }
    }
}
