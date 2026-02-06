using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using InterpolationApp.Models;

namespace InterpolationApp.Services
{
    /// <summary>
    /// Service for importing and exporting data in various formats
    /// </summary>
    public class DataService
    {
        /// <summary>
        /// Load data points from CSV file
        /// Expected format: X,Y (with or without header)
        /// </summary>
        public List<DataPoint> LoadFromCSV(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("CSV file not found", filePath);

            var points = new List<DataPoint>();
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Skip header line if it contains non-numeric data
                if (line.ToLower().Contains("x") && line.ToLower().Contains("y"))
                    continue;

                var parts = line.Split(',', ';', '\t');
                if (parts.Length >= 2)
                {
                    if (double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double x) &&
                        double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double y))
                    {
                        points.Add(new DataPoint(x, y));
                    }
                }
            }

            if (points.Count == 0)
                throw new InvalidDataException("No valid data points found in CSV file");

            return points;
        }

        /// <summary>
        /// Save data points to CSV file
        /// </summary>
        public void SaveToCSV(string filePath, List<DataPoint> points, bool includeHeader = true)
        {
            using var writer = new StreamWriter(filePath);
            
            if (includeHeader)
            {
                writer.WriteLine("X,Y");
            }

            foreach (var point in points)
            {
                writer.WriteLine($"{point.X.ToString(CultureInfo.InvariantCulture)},{point.Y.ToString(CultureInfo.InvariantCulture)}");
            }
        }

        /// <summary>
        /// Save interpolation results to CSV with algorithm names
        /// </summary>
        public void SaveResultsToCSV(string filePath, List<InterpolationResult> results)
        {
            using var writer = new StreamWriter(filePath);
            
            writer.WriteLine("X,Y,Algorithm,OriginalPoint");

            foreach (var result in results)
            {
                // Mark original points
                foreach (var point in result.OriginalPoints)
                {
                    writer.WriteLine($"{point.X.ToString(CultureInfo.InvariantCulture)},{point.Y.ToString(CultureInfo.InvariantCulture)},{result.AlgorithmName},True");
                }

                // Interpolated points
                foreach (var point in result.InterpolatedPoints)
                {
                    writer.WriteLine($"{point.X.ToString(CultureInfo.InvariantCulture)},{point.Y.ToString(CultureInfo.InvariantCulture)},{result.AlgorithmName},False");
                }
            }
        }

        /// <summary>
        /// Generate sample datasets for testing
        /// </summary>
        public List<DataPoint> GenerateSampleData(string functionName, int numPoints, double xMin, double xMax, double noiseLevel = 0)
        {
            var points = new List<DataPoint>();
            var random = new Random();
            double step = (xMax - xMin) / (numPoints - 1);

            for (int i = 0; i < numPoints; i++)
            {
                double x = xMin + i * step;
                double y = functionName.ToLower() switch
                {
                    "sin" => Math.Sin(x),
                    "cos" => Math.Cos(x),
                    "polynomial" => x * x * x - 2 * x * x + x - 3,
                    "exponential" => Math.Exp(-x * x),
                    "runge" => 1.0 / (1 + 25 * x * x),
                    _ => Math.Sin(x)
                };

                // Add noise if requested
                if (noiseLevel > 0)
                {
                    y += (random.NextDouble() - 0.5) * 2 * noiseLevel;
                }

                points.Add(new DataPoint(x, y));
            }

            return points;
        }

        /// <summary>
        /// Load data from simple text file (one value per line, or X Y pairs)
        /// </summary>
        public List<DataPoint> LoadFromTextFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Text file not found", filePath);

            var points = new List<DataPoint>();
            var lines = File.ReadAllLines(filePath);
            double x = 0;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 1)
                {
                    // Single value - use index as X
                    if (double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double y))
                    {
                        points.Add(new DataPoint(x++, y));
                    }
                }
                else if (parts.Length >= 2)
                {
                    // X Y pair
                    if (double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double xVal) &&
                        double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double yVal))
                    {
                        points.Add(new DataPoint(xVal, yVal));
                        x = xVal + 1;
                    }
                }
            }

            return points;
        }

        /// <summary>
        /// Export statistics and comparison report
        /// </summary>
        public void ExportComparisonReport(string filePath, List<InterpolationResult> results)
        {
            using var writer = new StreamWriter(filePath);

            writer.WriteLine("═══════════════════════════════════════════════════════");
            writer.WriteLine("     INTERPOLATION ALGORITHMS COMPARISON REPORT");
            writer.WriteLine("═══════════════════════════════════════════════════════");
            writer.WriteLine();
            writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            writer.WriteLine($"Number of algorithms compared: {results.Count}");
            writer.WriteLine();

            writer.WriteLine("───────────────────────────────────────────────────────");
            writer.WriteLine("  ALGORITHM PERFORMANCE METRICS");
            writer.WriteLine("───────────────────────────────────────────────────────");
            writer.WriteLine();

            // Sort by RMSE
            var sortedByRMSE = results.OrderBy(r => r.RMSError).ToList();

            writer.WriteLine("{0,-20} {1,15} {2,15} {3,15}", "Algorithm", "RMSE", "Max Error", "Time (ms)");
            writer.WriteLine(new string('─', 70));

            foreach (var result in sortedByRMSE)
            {
                writer.WriteLine("{0,-20} {1,15:F8} {2,15:F8} {3,15:F2}",
                    result.AlgorithmName,
                    result.RMSError,
                    result.MaxError,
                    result.ComputationTime.TotalMilliseconds);
            }

            writer.WriteLine();
            writer.WriteLine("───────────────────────────────────────────────────────");
            writer.WriteLine("  RANKING");
            writer.WriteLine("───────────────────────────────────────────────────────");
            writer.WriteLine();

            writer.WriteLine("By Accuracy (RMSE):");
            for (int i = 0; i < sortedByRMSE.Count; i++)
            {
                writer.WriteLine($"  {i + 1}. {sortedByRMSE[i].AlgorithmName} - RMSE: {sortedByRMSE[i].RMSError:F8}");
            }
            writer.WriteLine();

            var sortedBySpeed = results.OrderBy(r => r.ComputationTime).ToList();
            writer.WriteLine("By Speed:");
            for (int i = 0; i < sortedBySpeed.Count; i++)
            {
                writer.WriteLine($"  {i + 1}. {sortedBySpeed[i].AlgorithmName} - {sortedBySpeed[i].ComputationTime.TotalMilliseconds:F2} ms");
            }

            writer.WriteLine();
            writer.WriteLine("═══════════════════════════════════════════════════════");
            writer.WriteLine("               END OF REPORT");
            writer.WriteLine("═══════════════════════════════════════════════════════");
        }
    }
}
