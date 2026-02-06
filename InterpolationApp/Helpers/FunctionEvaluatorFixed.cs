using System;
using System.Data;
using System.Linq;
using InterpolationApp.Algorithms;

namespace InterpolationApp.Helpers
{
    /// <summary>
    /// FIXED - Using DataTable.Compute for reliable parsing
    /// Supports: +, -, *, /, ^, sin, cos, tan, exp, log, sqrt, abs, pi, e
    /// </summary>
    public class FunctionEvaluatorFixed
    {
        private readonly string _expression;
        private readonly DataTable _table;
        
        public FunctionEvaluatorFixed(string expression)
        {
            _expression = expression?.Trim() ?? throw new ArgumentNullException(nameof(expression));
            _table = new DataTable();
        }

        public double Evaluate(double x)
        {
            try
            {
                // Prepare expression
                string expr = _expression.ToLower();
                
                // Replace x with value
                expr = expr.Replace("x", $"({x})");
                
                // Replace constants
                expr = expr.Replace("pi", Math.PI.ToString("R"));
                expr = expr.Replace("e", Math.E.ToString("R"));
                
                // Handle functions that DataTable doesn't support
                expr = ProcessFunctions(expr, x);
                
                // Replace ^ with Power for DataTable
                expr = ConvertPowerSyntax(expr);
                
                // Evaluate using DataTable
                object result = _table.Compute(expr, "");
                return Convert.ToDouble(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Cannot evaluate '{_expression}' at x={x}: {ex.Message}", ex);
            }
        }

        private string ProcessFunctions(string expr, double x)
        {
            // Process trigonometric functions
            expr = ReplaceFunctionCalls(expr, "sin", Math.Sin);
            expr = ReplaceFunctionCalls(expr, "cos", Math.Cos);
            expr = ReplaceFunctionCalls(expr, "tan", Math.Tan);
            
            // Process other functions
            expr = ReplaceFunctionCalls(expr, "exp", Math.Exp);
            expr = ReplaceFunctionCalls(expr, "log", Math.Log);
            expr = ReplaceFunctionCalls(expr, "ln", Math.Log);
            expr = ReplaceFunctionCalls(expr, "sqrt", Math.Sqrt);
            expr = ReplaceFunctionCalls(expr, "abs", Math.Abs);
            
            return expr;
        }

        private string ReplaceFunctionCalls(string expr, string funcName, Func<double, double> func)
        {
            while (expr.Contains($"{funcName}("))
            {
                int start = expr.IndexOf($"{funcName}(");
                int end = FindMatchingBracket(expr, start + funcName.Length);
                
                string inner = expr.Substring(start + funcName.Length + 1, end - start - funcName.Length - 1);
                
                // Evaluate inner expression first
                double innerValue = EvaluateSimple(inner);
                double result = func(innerValue);
                
                expr = expr.Substring(0, start) + result.ToString("R") + expr.Substring(end + 1);
            }
            return expr;
        }

        private double EvaluateSimple(string expr)
        {
            try
            {
                // Simple evaluation for nested expressions
                expr = ConvertPowerSyntax(expr);
                object result = _table.Compute(expr, "");
                return Convert.ToDouble(result);
            }
            catch
            {
                // If DataTable can't parse it, try as literal number
                return double.Parse(expr);
            }
        }

        private string ConvertPowerSyntax(string expr)
        {
            // Convert x^2 to Power(x, 2) for DataTable
            while (expr.Contains("^"))
            {
                int pos = expr.IndexOf('^');
                
                // Extract left operand
                int leftStart = pos - 1;
                int parenDepth = 0;
                while (leftStart >= 0)
                {
                    if (expr[leftStart] == ')')
                        parenDepth++;
                    else if (expr[leftStart] == '(')
                    {
                        parenDepth--;
                        if (parenDepth == 0)
                            break;
                    }
                    else if (parenDepth == 0 && !char.IsDigit(expr[leftStart]) && expr[leftStart] != '.')
                        break;
                    
                    leftStart--;
                }
                leftStart++;
                
                string left = expr.Substring(leftStart, pos - leftStart);
                
                // Extract right operand
                int rightEnd = pos + 1;
                parenDepth = 0;
                while (rightEnd < expr.Length)
                {
                    if (expr[rightEnd] == '(')
                        parenDepth++;
                    else if (expr[rightEnd] == ')')
                    {
                        parenDepth--;
                        if (parenDepth < 0)
                            break;
                    }
                    else if (parenDepth == 0 && !char.IsDigit(expr[rightEnd]) && expr[rightEnd] != '.')
                        break;
                    
                    rightEnd++;
                }
                
                string right = expr.Substring(pos + 1, rightEnd - pos - 1);
                
                // Replace with Power function
                string powerExpr = $"Power({left}, {right})";
                expr = expr.Substring(0, leftStart) + powerExpr + expr.Substring(rightEnd);
            }
            
            return expr;
        }

        private int FindMatchingBracket(string expr, int openPos)
        {
            int depth = 1;
            for (int i = openPos + 1; i < expr.Length; i++)
            {
                if (expr[i] == '(') depth++;
                if (expr[i] == ')') depth--;
                if (depth == 0) return i;
            }
            throw new ArgumentException("Mismatched brackets");
        }

        // Generate Chebyshev nodes samples
        public (double[] x, double[] y) GenerateChebyshevSamples(double xMin, double xMax, int n)
        {
            double[] x = new double[n];
            double[] y = new double[n];

            for (int i = 0; i < n; i++)
            {
                double theta = (2.0 * i + 1) * Math.PI / (2.0 * n);
                x[i] = (xMin + xMax) / 2.0 + (xMax - xMin) / 2.0 * Math.Cos(theta);
                y[i] = Evaluate(x[i]);
            }

            var sorted = x.Zip(y, (xVal, yVal) => new { X = xVal, Y = yVal })
                          .OrderBy(p => p.X)
                          .ToArray();
            return (sorted.Select(p => p.X).ToArray(), sorted.Select(p => p.Y).ToArray());
        }

        // Generate uniform samples
        public (double[] x, double[] y) GenerateUniformSamples(double xMin, double xMax, int n)
        {
            double[] x = new double[n];
            double[] y = new double[n];
            double step = (xMax - xMin) / (n - 1);

            for (int i = 0; i < n; i++)
            {
                x[i] = xMin + i * step;
                y[i] = Evaluate(x[i]);
            }

            return (x, y);
        }

        // Calculate max error
        public double CalculateMaxError(IInterpolationAlgorithm interpolator, 
                                       double xMin, double xMax, int testPoints = 1000)
        {
            double maxError = 0;
            double step = (xMax - xMin) / (testPoints - 1);

            for (int i = 0; i < testPoints; i++)
            {
                double x = xMin + i * step;
                double actual = Evaluate(x);
                double interpolated = interpolator.Interpolate(x);
                double error = Math.Abs(actual - interpolated);
                maxError = Math.Max(maxError, error);
            }

            return maxError;
        }
    }
}
