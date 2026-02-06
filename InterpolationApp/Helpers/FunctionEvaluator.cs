using System;
using System.Diagnostics;
using System.Linq;
using InterpolationApp.Algorithms;

namespace InterpolationApp.Helpers
{
    /// <summary>
    /// Mathematical function parser and evaluator
    /// Supports: sin, cos, tan, exp, log, sqrt, abs, x^n, pi, e
    /// Examples: "sin(x)", "x^2", "exp(-x^2)", "cos(2*pi*x)"
    /// </summary>
    public class FunctionEvaluator
    {
        private readonly string _expression;
        
        public FunctionEvaluator(string expression)
        {
            _expression = expression?.Trim().ToLower() ?? throw new ArgumentNullException(nameof(expression));
        }

        public double Evaluate(double x)
        {
            try
            {
                Console.WriteLine($"[PARSER] Evaluating: {_expression} at x={x}");
                string expr = _expression.Replace("x", $"({x:R})");
                Console.WriteLine($"[PARSER] After x substitution: {expr}");
                expr = expr.Replace("pi", Math.PI.ToString("R"));
                expr = expr.Replace("e", "2.718281828459045");
                expr = expr.Replace(" ", "");
                Console.WriteLine($"[PARSER] Before evaluation: {expr}");
                
                double result = EvaluateExpression(expr);
                Console.WriteLine($"[PARSER] Result: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PARSER ERROR] Expression: '{_expression}'");
                Console.WriteLine($"[PARSER ERROR] At x={x}");
                Console.WriteLine($"[PARSER ERROR] Message: {ex.Message}");
                Console.WriteLine($"[PARSER ERROR] Type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[PARSER ERROR] Inner: {ex.InnerException.Message}");
                throw new InvalidOperationException($"Cannot evaluate '{_expression}' at x={x}: {ex.Message}", ex);
            }
        }

        private double EvaluateExpression(string expr)
        {
            expr = ProcessFunctions(expr);
            return ParseArithmetic(expr);
        }

        private string ProcessFunctions(string expr)
        {
            // Sin
            while (expr.Contains("sin("))
            {
                int start = expr.IndexOf("sin(");
                int end = FindMatchingBracket(expr, start + 3);
                string inner = expr.Substring(start + 4, end - start - 4);
                double result = Math.Sin(ParseArithmetic(inner));
                expr = expr.Substring(0, start) + result.ToString("R") + expr.Substring(end + 1);
            }

            // Cos
            while (expr.Contains("cos("))
            {
                int start = expr.IndexOf("cos(");
                int end = FindMatchingBracket(expr, start + 3);
                string inner = expr.Substring(start + 4, end - start - 4);
                double result = Math.Cos(ParseArithmetic(inner));
                expr = expr.Substring(0, start) + result.ToString("R") + expr.Substring(end + 1);
            }

            // Tan
            while (expr.Contains("tan("))
            {
                int start = expr.IndexOf("tan(");
                int end = FindMatchingBracket(expr, start + 3);
                string inner = expr.Substring(start + 4, end - start - 4);
                double result = Math.Tan(ParseArithmetic(inner));
                expr = expr.Substring(0, start) + result.ToString("R") + expr.Substring(end + 1);
            }

            // Exp
            while (expr.Contains("exp("))
            {
                int start = expr.IndexOf("exp(");
                int end = FindMatchingBracket(expr, start + 3);
                string inner = expr.Substring(start + 4, end - start - 4);
                double result = Math.Exp(ParseArithmetic(inner));
                expr = expr.Substring(0, start) + result.ToString("R") + expr.Substring(end + 1);
            }

            // Log/Ln
            while (expr.Contains("log(") || expr.Contains("ln("))
            {
                bool isLog = expr.Contains("log(");
                int start = expr.IndexOf(isLog ? "log(" : "ln(");
                int offset = isLog ? 3 : 2;
                int end = FindMatchingBracket(expr, start + offset);
                string inner = expr.Substring(start + offset + 1, end - start - offset - 1);
                double result = Math.Log(ParseArithmetic(inner));
                expr = expr.Substring(0, start) + result.ToString("R") + expr.Substring(end + 1);
            }

            // Sqrt
            while (expr.Contains("sqrt("))
            {
                int start = expr.IndexOf("sqrt(");
                int end = FindMatchingBracket(expr, start + 4);
                string inner = expr.Substring(start + 5, end - start - 5);
                double result = Math.Sqrt(ParseArithmetic(inner));
                expr = expr.Substring(0, start) + result.ToString("R") + expr.Substring(end + 1);
            }

            // Abs
            while (expr.Contains("abs("))
            {
                int start = expr.IndexOf("abs(");
                int end = FindMatchingBracket(expr, start + 3);
                string inner = expr.Substring(start + 4, end - start - 4);
                double result = Math.Abs(ParseArithmetic(inner));
                expr = expr.Substring(0, start) + result.ToString("R") + expr.Substring(end + 1);
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
            throw new ArgumentException("Mismatched brackets in expression");
        }

        private double ParseArithmetic(string expr)
        {
            Console.WriteLine($"[PARSE] Parsing arithmetic: {expr}");
            try
            {
                // Handle brackets first
                while (expr.Contains("("))
            {
                int start = expr.LastIndexOf('(');
                int end = expr.IndexOf(')', start);
                string inner = expr.Substring(start + 1, end - start - 1);
                double result = ParseArithmetic(inner);
                expr = expr.Substring(0, start) + result.ToString("R") + expr.Substring(end + 1);
            }

            // Handle power (^)
            while (expr.Contains("^"))
            {
                int pos = expr.IndexOf('^');
                var (left, leftStart) = ExtractLeftNumber(expr, pos);
                var (right, rightEnd) = ExtractRightNumber(expr, pos);
                double result = Math.Pow(left, right);
                expr = expr.Substring(0, leftStart) + result.ToString("R") + expr.Substring(rightEnd);
            }

            // Handle * and /
            while (expr.Contains("*") || expr.Contains("/"))
            {
                int mulPos = expr.IndexOf('*');
                int divPos = expr.IndexOf('/');
                int pos = (mulPos >= 0 && divPos >= 0) ? Math.Min(mulPos, divPos) : (mulPos >= 0 ? mulPos : divPos);
                char op = expr[pos];
                
                var (left, leftStart) = ExtractLeftNumber(expr, pos);
                var (right, rightEnd) = ExtractRightNumber(expr, pos);
                double result = op == '*' ? left * right : left / right;
                expr = expr.Substring(0, leftStart) + result.ToString("R") + expr.Substring(rightEnd);
            }

            // Handle + and -
            double sum = 0;
            int i = 0;
            int sign = 1;

            if (expr.Length > 0 && expr[0] == '-')
            {
                sign = -1;
                i = 1;
            }
            else if (expr.Length > 0 && expr[0] == '+')
            {
                i = 1;
            }

            while (i < expr.Length)
            {
                int numEnd = i;
                while (numEnd < expr.Length && (char.IsDigit(expr[numEnd]) || expr[numEnd] == '.' || expr[numEnd] == 'e' || expr[numEnd] == 'E' || (numEnd > i && (expr[numEnd] == '+' || expr[numEnd] == '-'))))
                    numEnd++;

                if (numEnd > i)
                {
                    string numStr = expr.Substring(i, numEnd - i);
                    double num = double.Parse(numStr);
                    sum += sign * num;
                    i = numEnd;
                }

                if (i < expr.Length)
                {
                    if (expr[i] == '+') sign = 1;
                    else if (expr[i] == '-') sign = -1;
                    i++;
                }
            }

            return sum;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PARSE ERROR] Failed to parse: {expr}");
                Console.WriteLine($"[PARSE ERROR] Message: {ex.Message}");
                Console.WriteLine($"[PARSE ERROR] Stack: {ex.StackTrace}");
                throw;
            }
        }

        private (double value, int start) ExtractLeftNumber(string expr, int opPos)
        {
            int end = opPos - 1;
            while (end >= 0 && (char.IsDigit(expr[end]) || expr[end] == '.' || expr[end] == 'e' || expr[end] == 'E'))
                end--;
            
            int start = end + 1;
            if (start > 0 && expr[start - 1] == '-')
                start--;

            string numStr = expr.Substring(start, opPos - start);
            Console.WriteLine($"[EXTRACT LEFT] From '{expr}' at pos {opPos}: '{numStr}'");
            try
            {
                return (double.Parse(numStr), start);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXTRACT LEFT ERROR] Cannot parse '{numStr}': {ex.Message}");
                throw;
            }
        }

        private (double value, int end) ExtractRightNumber(string expr, int opPos)
        {
            int start = opPos + 1;
            if (start < expr.Length && expr[start] == '-')
                start++;
            
            int end = start;
            while (end < expr.Length && (char.IsDigit(expr[end]) || expr[end] == '.' || expr[end] == 'e' || expr[end] == 'E'))
                end++;

            string numStr = expr.Substring(opPos + 1, end - opPos - 1);
            Console.WriteLine($"[EXTRACT RIGHT] From '{expr}' at pos {opPos}: '{numStr}'");
            try
            {
                return (double.Parse(numStr), end);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXTRACT RIGHT ERROR] Cannot parse '{numStr}': {ex.Message}");
                throw;
            }
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

            // Sort by x
            var sorted = x.Zip(y, (xVal, yVal) => new { X = xVal, Y = yVal }).OrderBy(p => p.X).ToArray();
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

        // Calculate interpolation error - FIXED namespace
        public double CalculateMaxError(IInterpolationAlgorithm interpolator, double xMin, double xMax, int testPoints = 1000)
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
