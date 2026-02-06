using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using OxyPlot.Annotations;

// CLEAR ALIASES - bez ambiguity
using AppDataPoint = InterpolationApp.Models.DataPoint;
using AppInterpolationResult = InterpolationApp.Models.InterpolationResult;
using AppIInterpolationAlgorithm = InterpolationApp.Algorithms.IInterpolationAlgorithm;
using InterpolationApp.Algorithms;
using InterpolationApp.Helpers;
using InterpolationApp.Services;

namespace InterpolationApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly AudioProcessingService _audioService = new();
        private float[]? _audioSamples;
        private int _audioSampleRate;
        private int _audioChannels;

        [ObservableProperty]
        private ObservableCollection<AppDataPoint> dataPoints = new();

        // String properties za input - prihvataju i zarez i tačku
        [ObservableProperty]
        private string inputXText = "0";

        [ObservableProperty]
        private string inputYText = "0";

        // PlotModel for Tab 1 (Manual Input)
        public PlotModel PlotModel { get; set; }
        
        // FunctionPlotModel for Tab 2 (Function Sampling)
        public PlotModel FunctionPlotModel { get; set; }
        
        // ErrorPlotModel for Tab 3 (Degree Optimizer)
        public PlotModel ErrorPlotModel { get; set; }

        [ObservableProperty]
        private string selectedAlgorithm = "Cubic Spline";

        [ObservableProperty]
        private int interpolationPointsCount = 200;

        [ObservableProperty]
        private string statusMessage = "Ready";

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private ObservableCollection<AppInterpolationResult> results = new();

        [ObservableProperty]
        private string functionExpression = "sin(x)"; // Default: sin(x), možete koristiti i x*x, exp(-x*x), 1/(1+25*x*x), itd.

        // String properties za fleksibilan unos - prihvataju i zarez i tačku
        [ObservableProperty]
        private string xMinText = "0";

        [ObservableProperty]
        private string xMaxText = "6.28";

        [ObservableProperty]
        private int polynomialDegree = 10;

        [ObservableProperty]
        private string testPointsCountText = "1000";

        [ObservableProperty]
        private bool useChebyshevNodes = true;

        [ObservableProperty]
        private string targetErrorText = "0.001";

        [ObservableProperty]
        private string minDegreeText = "2";

        [ObservableProperty]
        private string maxDegreeText = "30";

        [ObservableProperty]
        private int optimalDegree;

        [ObservableProperty]
        private double actualError;

        [ObservableProperty]
        private string? optimizerResult;

        [ObservableProperty]
        private ObservableCollection<DegreeErrorResult> degreeAnalysisResults = new();

        [ObservableProperty]
        private bool isAudioLoaded;

        [ObservableProperty]
        private string audioFileName = "No file loaded";

        [ObservableProperty]
        private int audioSampleCount;

        [ObservableProperty]
        private double noiseThreshold = 3.0;
        
        // Evaluate at Point feature
        [ObservableProperty]
        private string evaluateXText = "0";
        
        [ObservableProperty]
        private ObservableCollection<EvaluationResult> evaluatedResults = new();

        public ObservableCollection<string> AvailableAlgorithms { get; } = new()
        {
            "Lagrange",
            "Newton",
            "Cubic Spline",
            "Hermite",
            "Linear"
        };

        public MainViewModel()
        {
            // Create PlotModel for Tab 1 (Manual Input)
            PlotModel = new PlotModel
            {
                Title = "Interpolation Visualization (Hover to preview • Click to pin)",
                Background = OxyColors.White,
                TextColor = OxyColors.Black,
                PlotAreaBorderColor = OxyColors.LightGray,
                TitleFontSize = 14
            };
            
            // Add mouse click handler for sticky tooltip
            PlotModel.MouseDown += OnPlotModelMouseDown;
            
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "X",
                TitleColor = OxyColors.Black,
                TextColor = OxyColors.Black,
                AxislineColor = OxyColors.Gray,
                TicklineColor = OxyColors.Gray,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromRgb(240, 240, 240)
            });

            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Y",
                TitleColor = OxyColors.Black,
                TextColor = OxyColors.Black,
                AxislineColor = OxyColors.Gray,
                TicklineColor = OxyColors.Gray,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromRgb(240, 240, 240)
            });
            
            // Create FunctionPlotModel for Tab 2 (Function Sampling)
            FunctionPlotModel = new PlotModel
            {
                Title = "Function vs Interpolations (Hover to preview • Click to pin)",
                Background = OxyColors.White,
                TextColor = OxyColors.Black,
                PlotAreaBorderColor = OxyColors.LightGray,
                TitleFontSize = 14
            };
            
            // Add mouse click handler for sticky tooltip
            FunctionPlotModel.MouseDown += OnFunctionPlotModelMouseDown;
            
            FunctionPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "X",
                TitleColor = OxyColors.Black,
                TextColor = OxyColors.Black,
                AxislineColor = OxyColors.Gray,
                TicklineColor = OxyColors.Gray,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromRgb(240, 240, 240)
            });

            FunctionPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Y",
                TitleColor = OxyColors.Black,
                TextColor = OxyColors.Black,
                AxislineColor = OxyColors.Gray,
                TicklineColor = OxyColors.Gray,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromRgb(240, 240, 240)
            });
            
            // Create ErrorPlotModel for Tab 3 (Degree Optimizer)
            ErrorPlotModel = new PlotModel
            {
                Title = "Error vs Polynomial Degree",
                Background = OxyColors.White,
                TextColor = OxyColors.Black
            };

            ErrorPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Polynomial Degree (n)",
                TitleColor = OxyColors.Black,
                TextColor = OxyColors.Black
            });

            ErrorPlotModel.Axes.Add(new LogarithmicAxis
            {
                Position = AxisPosition.Left,
                Title = "Maximum Error (log scale)",
                TitleColor = OxyColors.Black,
                TextColor = OxyColors.Black
            });
        }

        [RelayCommand]
        private void AddDataPoint()
        {
            try
            {
                // Parsiranje sa podrškom za i zarez i tačku
                double x = ParseFlexibleDouble(InputXText);
                double y = ParseFlexibleDouble(InputYText);
                
                DataPoints.Add(new AppDataPoint(x, y));
                StatusMessage = $"Added point: ({x:F2}, {y:F2}) - Total: {DataPoints.Count}";
                ErrorMessage = string.Empty;
                
                // ✅ RESET: Podaci su se promijenili - resetuj sve rezultate
                ClearIntermediateResults();
                
                // Increment InputX for next point
                InputXText = (x + 1.0).ToString(System.Globalization.CultureInfo.CurrentCulture);
                
                // Update plot to show new point
                UpdatePlot();
            }
            catch (FormatException)
            {
                ErrorMessage = "Invalid number format. Use numbers like: 3.14 or 3,14";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                MessageBox.Show(
                    $"Failed to add point:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Čisti evaluaciju i polynomial equation rezultate jer se podaci promijenili
        /// </summary>
        private void ClearIntermediateResults()
        {
            // Očisti evaluated results
            EvaluatedResults.Clear();
            
            // Očisti polynomial equations iz svih rezultata
            foreach (var result in Results)
            {
                result.PolynomialEquation = string.Empty;
            }
        }
        
        /// <summary>
        /// Parsira string u double, prihvatajući i zarez (,) i tačku (.) kao decimalni separator
        /// </summary>
        private double ParseFlexibleDouble(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new FormatException("Input cannot be empty");
                
            // Prvo pokušaj sa trenutnom kulturom
            if (double.TryParse(input, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.CurrentCulture, out double result))
            {
                return result;
            }

            // Ako ne uspe, pokušaj sa invariant kulturom (tačka kao separator)
            if (double.TryParse(input, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out result))
            {
                return result;
            }

            // Pokušaj zamijeniti separator i ponovo parsirati
            string normalized = input.Replace(',', '.').Replace(" ", "");
            if (double.TryParse(normalized, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out result))
            {
                return result;
            }

            throw new FormatException($"Cannot parse '{input}' as a number");
        }
        
        /// <summary>
        /// Parsira string u int, prihvatajući fleksibilan unos
        /// </summary>
        private int ParseFlexibleInt(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new FormatException("Input cannot be empty");
                
            // Prvo pokušaj sa trenutnom kulturom
            if (int.TryParse(input, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.CurrentCulture, out int result))
            {
                return result;
            }

            // Ako ne uspe, pokušaj sa invariant kulturom
            if (int.TryParse(input, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out result))
            {
                return result;
            }

            // Ukloni razmake i pokušaj ponovo
            string normalized = input.Replace(" ", "");
            if (int.TryParse(normalized, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out result))
            {
                return result;
            }

            throw new FormatException($"Cannot parse '{input}' as an integer");
        }

        [RelayCommand]
        private void RemoveLastPoint()
        {
            if (DataPoints.Count > 0)
            {
                DataPoints.RemoveAt(DataPoints.Count - 1);
                StatusMessage = "Removed last point";
                
                // ✅ RESET: Podaci su se promijenili
                ClearIntermediateResults();
                UpdatePlot();
            }
        }

        [RelayCommand]
        private void ClearPoints()
        {
            DataPoints.Clear();
            Results.Clear(); // ✅ Obriši sve interpolacione rezultate
            EvaluatedResults.Clear(); // ✅ Obriši evaluated results
            ErrorMessage = string.Empty; // ✅ Obriši error poruke
            StatusMessage = "Cleared all points and results";
            UpdatePlot();
        }

        [RelayCommand]
        private void AddSampleData()
        {
            DataPoints.Clear();
            Results.Clear(); // ✅ Clear old results
            EvaluatedResults.Clear(); // ✅ Clear evaluated results
            
            var random = new Random();
            for (int i = 0; i < 10; i++)
            {
                double x = i;
                double y = Math.Sin(x * 0.8) * 10 + (random.NextDouble() - 0.5) * 2;
                DataPoints.Add(new AppDataPoint(x, y));
            }
            StatusMessage = "Added sample data";
            UpdatePlot();
        }

        [RelayCommand]
        private void GenerateRandomData()
        {
            try
            {
                DataPoints.Clear();
                Results.Clear(); // ✅ Clear old results
                EvaluatedResults.Clear(); // ✅ Clear evaluated results
                
                var random = new Random();
                int count = 15;
                for (int i = 0; i < count; i++)
                {
                    double x = i * 2.0;
                    double y = random.NextDouble() * 20 - 10;
                    DataPoints.Add(new AppDataPoint(x, y));
                }
                StatusMessage = $"Generated {count} random points";
                UpdatePlot();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }

        [RelayCommand]
        private void Interpolate()
        {
            try
            {
                // Check minimum points required for selected algorithm
                int minPoints = SelectedAlgorithm == "Cubic Spline" || SelectedAlgorithm == "Hermite" ? 3 : 2;
                
                if (DataPoints.Count < minPoints)
                {
                    ErrorMessage = $"'{SelectedAlgorithm}' needs at least {minPoints} data points! (You have {DataPoints.Count})";
                    return;
                }

                ErrorMessage = string.Empty;
                
                // ✅ LOGIKA: Kada interpoliraš jedan algoritam, obriši sve stare rezultate
                Results.Clear();
                
                // ✅ Clear evaluated results since we're running new interpolation
                EvaluatedResults.Clear();
                
                var stopwatch = Stopwatch.StartNew();
                var algorithm = CreateAlgorithm(SelectedAlgorithm);
                
                double[] xPoints = DataPoints.Select(p => p.X).ToArray();
                double[] yPoints = DataPoints.Select(p => p.Y).ToArray();
                algorithm.SetData(xPoints, yPoints);

                double xMinVal = xPoints.Min();
                double xMaxVal = xPoints.Max();
                var interpolatedY = algorithm.InterpolateRange(xMinVal, xMaxVal, InterpolationPointsCount);
                stopwatch.Stop();

                var result = new AppInterpolationResult
                {
                    AlgorithmName = SelectedAlgorithm,
                    OriginalPoints = DataPoints.ToList(),
                    InterpolatedPoints = new List<AppDataPoint>(),
                    ComputationTime = stopwatch.Elapsed,
                    IsVisible = true  // Novi rezultat je vidljiv
                };

                double step = (xMaxVal - xMinVal) / (InterpolationPointsCount - 1);
                for (int i = 0; i < InterpolationPointsCount; i++)
                {
                    double x = xMinVal + i * step;
                    result.InterpolatedPoints.Add(new AppDataPoint(x, interpolatedY[i]));
                }

                result.RMSError = algorithm.CalculateError(xPoints, yPoints);
                result.MaxError = xPoints.Select((x, i) => Math.Abs(algorithm.Interpolate(x) - yPoints[i])).Max();
                
                // Generiši jednačinu za Lagrange i Newton
                if (SelectedAlgorithm == "Lagrange" || SelectedAlgorithm == "Newton")
                {
                    result.PolynomialEquation = GeneratePolynomialEquation(algorithm, xPoints, yPoints);
                }
                
                // ✅ Ažuriraj grafik kada se promijeni visibility
                result.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(AppInterpolationResult.IsVisible))
                    {
                        UpdatePlot();
                    }
                };
                
                Results.Add(result);
                StatusMessage = $"Interpolation complete: {stopwatch.ElapsedMilliseconds}ms, RMSE: {result.RMSError:F6}";
                UpdatePlot();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Interpolation error: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CompareAlgorithms()
        {
            try
            {
                if (DataPoints.Count < 3)
                {
                    ErrorMessage = "Need at least 3 data points to compare all algorithms!";
                    return;
                }

                ErrorMessage = string.Empty;
                Results.Clear();
                EvaluatedResults.Clear(); // Clear evaluated results
                
                double[] xPoints = DataPoints.Select(p => p.X).ToArray();
                double[] yPoints = DataPoints.Select(p => p.Y).ToArray();

                foreach (var algoName in AvailableAlgorithms)
                {
                    try
                    {
                        var stopwatch = Stopwatch.StartNew();
                        var algorithm = CreateAlgorithm(algoName);
                        algorithm.SetData(xPoints, yPoints);

                        double xMinVal = xPoints.Min();
                        double xMaxVal = xPoints.Max();
                        var interpolatedY = algorithm.InterpolateRange(xMinVal, xMaxVal, InterpolationPointsCount);
                        stopwatch.Stop();

                        var result = new AppInterpolationResult
                        {
                            AlgorithmName = algoName,
                            OriginalPoints = DataPoints.ToList(),
                            InterpolatedPoints = new List<AppDataPoint>(),
                            ComputationTime = stopwatch.Elapsed,
                            IsVisible = true  // LOGIKA: U COMPARE ALL, svi su vidljivi
                        };

                        double step = (xMaxVal - xMinVal) / (InterpolationPointsCount - 1);
                        for (int i = 0; i < InterpolationPointsCount; i++)
                        {
                            double x = xMinVal + i * step;
                            result.InterpolatedPoints.Add(new AppDataPoint(x, interpolatedY[i]));
                        }

                        result.RMSError = algorithm.CalculateError(xPoints, yPoints);
                        result.MaxError = xPoints.Select((x, i) => Math.Abs(algorithm.Interpolate(x) - yPoints[i])).Max();
                        
                        // Generiši jednačinu za Lagrange i Newton
                        if (algoName == "Lagrange" || algoName == "Newton")
                        {
                            result.PolynomialEquation = GeneratePolynomialEquation(algorithm, xPoints, yPoints);
                        }
                        
                        // ✅ Ažuriraj grafik kada se promijeni visibility
                        result.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == nameof(AppInterpolationResult.IsVisible))
                            {
                                UpdatePlot();
                            }
                        };
                        
                        Results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage += $"\n{algoName}: {ex.Message}";
                    }
                }

                StatusMessage = $"Compared {Results.Count} algorithms";
                UpdatePlot();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Comparison error: {ex.Message}";
            }
        }

        [RelayCommand]
        private void GenerateSamples()
        {
            try
            {
                ErrorMessage = string.Empty;
                Results.Clear();

                if (string.IsNullOrWhiteSpace(FunctionExpression))
                {
                    ErrorMessage = "Please enter a function!";
                    return;
                }
                
                // Parsiranje input vrednosti
                double xMin, xMax;
                int testPoints;
                try
                {
                    xMin = ParseFlexibleDouble(XMinText);
                    xMax = ParseFlexibleDouble(XMaxText);
                    testPoints = ParseFlexibleInt(TestPointsCountText);
                    
                    if (xMin >= xMax)
                    {
                        ErrorMessage = "XMin must be less than XMax!";
                        return;
                    }
                    
                    if (testPoints < 10)
                    {
                        ErrorMessage = "Test points must be at least 10!";
                        return;
                    }
                }
                catch (FormatException ex)
                {
                    ErrorMessage = $"Invalid input: {ex.Message}";
                    return;
                }

                FunctionEvaluatorFixed function;
                try
                {
                    function = new FunctionEvaluatorFixed(FunctionExpression);
                    function.Evaluate((xMin + xMax) / 2);
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Invalid function: {ex.Message}";
                    return;
                }

                StatusMessage = "Generating samples and interpolating...";
                Console.WriteLine($"[DEBUG] Function: {FunctionExpression}");
                Console.WriteLine($"[DEBUG] Interval: [{xMin}, {xMax}]");
                Console.WriteLine($"[DEBUG] Degree: {PolynomialDegree}");
                Console.WriteLine($"[DEBUG] Chebyshev: {UseChebyshevNodes}");

                var (xSamples, ySamples) = UseChebyshevNodes 
                    ? function.GenerateChebyshevSamples(xMin, xMax, PolynomialDegree)
                    : function.GenerateUniformSamples(xMin, xMax, PolynomialDegree);
                
                Console.WriteLine($"[DEBUG] Generated {xSamples.Length} samples");

                DataPoints.Clear();
                for (int i = 0; i < xSamples.Length; i++)
                {
                    DataPoints.Add(new AppDataPoint(xSamples[i], ySamples[i]));
                }

                foreach (var algoName in AvailableAlgorithms)
                {
                    try
                    {
                        Console.WriteLine($"[DEBUG] Processing algorithm: {algoName}");
                        var stopwatch = Stopwatch.StartNew();
                        var algorithm = CreateAlgorithm(algoName);
                        algorithm.SetData(xSamples, ySamples);
                        var interpolatedY = algorithm.InterpolateRange(xMin, xMax, InterpolationPointsCount);
                        stopwatch.Stop();

                        var result = new AppInterpolationResult
                        {
                            AlgorithmName = algoName,
                            OriginalPoints = DataPoints.ToList(),
                            InterpolatedPoints = new List<AppDataPoint>(),
                            ComputationTime = stopwatch.Elapsed,
                            IsVisible = true  // LOGIKA: U FUNCTION SAMPLING, svi su vidljivi na početku
                        };

                        double step = (xMax - xMin) / (InterpolationPointsCount - 1);
                        for (int i = 0; i < InterpolationPointsCount; i++)
                        {
                            double x = xMin + i * step;
                            result.InterpolatedPoints.Add(new AppDataPoint(x, interpolatedY[i]));
                        }

                        result.MaxError = function.CalculateMaxError(algorithm, xMin, xMax, testPoints);
                        result.RMSError = CalculateRMSEAgainstFunction(algorithm, function, xMin, xMax, testPoints);
                        
                        // ✅ Ažuriraj grafik kada se promijeni visibility
                        result.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == nameof(AppInterpolationResult.IsVisible))
                            {
                                UpdatePlotWithRealFunction(function, xMin, xMax);
                            }
                        };
                        
                        Results.Add(result);
                        Console.WriteLine($"[DEBUG] {algoName} completed - MaxError: {result.MaxError:E2}, Points: {result.InterpolatedPoints.Count}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] {algoName}: {ex.Message}");
                        Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                    }
                }

                UpdatePlotWithRealFunction(function, xMin, xMax);
                StatusMessage = $"Generated {PolynomialDegree} samples, interpolated with {Results.Count} algorithms";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Parser Error: {ex.Message}";
                
                // Show detailed error popup
                string details = $"Function: {FunctionExpression}\n" +
                               $"Interval: [{XMinText}, {XMaxText}]\n" +
                               $"Error: {ex.Message}";
                               
                if (ex.InnerException != null)
                    details += $"\n\nInner Error: {ex.InnerException.Message}";
                    
                MessageBox.Show(details, "Function Parser Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private double CalculateRMSEAgainstFunction(AppIInterpolationAlgorithm algorithm, FunctionEvaluatorFixed function, 
                                                     double xMin, double xMax, int testPoints)
        {
            double sumSquaredError = 0;
            double step = (xMax - xMin) / (testPoints - 1);

            for (int i = 0; i < testPoints; i++)
            {
                double x = xMin + i * step;
                double actual = function.Evaluate(x);
                double interpolated = algorithm.Interpolate(x);
                double error = actual - interpolated;
                sumSquaredError += error * error;
            }

            return Math.Sqrt(sumSquaredError / testPoints);
        }

        private void UpdatePlotWithRealFunction(FunctionEvaluatorFixed function, double xMin, double xMax)
        {
            Console.WriteLine($"[DEBUG] UpdatePlotWithRealFunction called");
            Console.WriteLine($"[DEBUG] Interval: [{xMin}, {xMax}]");
            Console.WriteLine($"[DEBUG] Results count: {Results.Count}");
            
            FunctionPlotModel.Series.Clear();
            
            // Clear sticky annotation when updating plot
            if (_currentFunctionAnnotation != null)
            {
                FunctionPlotModel.Annotations.Remove(_currentFunctionAnnotation);
                _currentFunctionAnnotation = null;
            }

            var realFunctionSeries = new LineSeries
            {
                Title = "Real Function",
                Color = OxyColors.Red,
                StrokeThickness = 3,
                LineStyle = LineStyle.Solid,
                TrackerFormatString = "Real Function\nX: {2:0.####}\nY: {4:0.####}"
            };

            double step = (xMax - xMin) / (InterpolationPointsCount - 1);
            for (int i = 0; i < InterpolationPointsCount; i++)
            {
                double x = xMin + i * step;
                double y = function.Evaluate(x);
                realFunctionSeries.Points.Add(new OxyPlot.DataPoint(x, y));
            }

            FunctionPlotModel.Series.Add(realFunctionSeries);

            var pointsSeries = new ScatterSeries
            {
                Title = "Sample Points",
                MarkerType = MarkerType.Circle,
                MarkerSize = 6,
                MarkerFill = OxyColors.Black,
                MarkerStroke = OxyColors.White,
                MarkerStrokeThickness = 1,
                TrackerFormatString = "Sample Points\nX: {2:0.####}\nY: {4:0.####}"
            };

            foreach (var point in DataPoints)
            {
                pointsSeries.Points.Add(new ScatterPoint(point.X, point.Y));
            }

            FunctionPlotModel.Series.Add(pointsSeries);

            var colors = new[] { OxyColors.Blue, OxyColors.Green, OxyColors.Orange, 
                               OxyColors.Purple, OxyColors.Brown };
            int colorIndex = 0;

            // ✅ Prikaži SAMO vidljive rezultate
            foreach (var result in Results.Where(r => r.IsVisible))
            {
                var lineSeries = new LineSeries
                {
                    Title = $"{result.AlgorithmName} (err: {result.MaxError:E2})",
                    Color = colors[colorIndex % colors.Length],
                    StrokeThickness = 2,
                    LineStyle = LineStyle.Dash,
                    TrackerFormatString = $"{result.AlgorithmName}\n━━━━━━━━━━━━\nX: {{2:0.####}}\nY: {{4:0.####}}\nError: {result.MaxError:E2}",
                    CanTrackerInterpolatePoints = false
                };

                foreach (var point in result.InterpolatedPoints)
                {
                    lineSeries.Points.Add(new OxyPlot.DataPoint(point.X, point.Y));
                }

                FunctionPlotModel.Series.Add(lineSeries);
                colorIndex++;
            }

            FunctionPlotModel.InvalidatePlot(true);
        }

        [RelayCommand]
        private void FindMinDegree()
        {
            try
            {
                ErrorMessage = string.Empty;
                DegreeAnalysisResults.Clear();

                if (string.IsNullOrWhiteSpace(FunctionExpression))
                {
                    ErrorMessage = "Please enter a function!";
                    return;
                }
                
                // Parsiranje input vrednosti
                double xMin, xMax, targetError;
                int minDegree, maxDegree;
                try
                {
                    xMin = ParseFlexibleDouble(XMinText);
                    xMax = ParseFlexibleDouble(XMaxText);
                    targetError = ParseFlexibleDouble(TargetErrorText);
                    minDegree = ParseFlexibleInt(MinDegreeText);
                    maxDegree = ParseFlexibleInt(MaxDegreeText);
                    
                    if (xMin >= xMax)
                    {
                        ErrorMessage = "XMin must be less than XMax!";
                        return;
                    }
                    
                    if (targetError <= 0)
                    {
                        ErrorMessage = "Target error must be positive!";
                        return;
                    }
                    
                    if (minDegree < 2)
                    {
                        ErrorMessage = "Min degree must be at least 2!";
                        return;
                    }
                    
                    if (maxDegree <= minDegree)
                    {
                        ErrorMessage = "Max degree must be greater than min degree!";
                        return;
                    }
                }
                catch (FormatException ex)
                {
                    ErrorMessage = $"Invalid input: {ex.Message}";
                    return;
                }

                FunctionEvaluatorFixed function;
                try
                {
                    function = new FunctionEvaluatorFixed(FunctionExpression);
                    function.Evaluate((xMin + xMax) / 2);
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Invalid function: {ex.Message}";
                    return;
                }

                StatusMessage = "Searching for optimal degree...";

                var optimizer = new PolynomialDegreeOptimizer(function, xMin, xMax, SelectedAlgorithm);
                var (minN, error, results) = optimizer.FindMinimumDegree(targetError, minDegree, maxDegree);

                OptimalDegree = minN;
                ActualError = error;

                if (error <= targetError)
                {
                    OptimizerResult = $"Found! Minimum degree n = {minN} achieves error ≤ {targetError:E6}";
                    StatusMessage = $"Success: n_min = {minN}, error = {error:E6}";
                }
                else
                {
                    OptimizerResult = $"Target not achieved. Best result: n = {minN} with error = {error:E6}";
                    StatusMessage = $"Target not reached. Try higher maxDegree or larger targetError.";
                }

                foreach (var (n, err) in results)
                {
                    DegreeAnalysisResults.Add(new DegreeErrorResult
                    {
                        Degree = n,
                        MaxError = err,
                        AvgError = err * 0.7
                    });
                }

                UpdateErrorPlot(results, targetError);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Optimizer error: {ex.Message}";
            }
        }

        private void UpdateErrorPlot(List<(int n, double error)> results, double targetError)
        {
            ErrorPlotModel.Series.Clear();

            var series = new LineSeries
            {
                Title = $"{SelectedAlgorithm} Error",
                Color = OxyColors.Blue,
                StrokeThickness = 2,
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerFill = OxyColors.Blue
            };

            foreach (var (n, error) in results)
            {
                series.Points.Add(new OxyPlot.DataPoint(n, error));
            }

            ErrorPlotModel.Series.Add(series);

            if (results.Count > 0)
            {
                var targetLine = new LineSeries
                {
                    Title = $"Target (ε = {targetError:E2})",
                    Color = OxyColors.Red,
                    StrokeThickness = 2,
                    LineStyle = LineStyle.Dash
                };

                int minN = results.Min(r => r.n);
                int maxN = results.Max(r => r.n);
                targetLine.Points.Add(new OxyPlot.DataPoint(minN, targetError));
                targetLine.Points.Add(new OxyPlot.DataPoint(maxN, targetError));

                ErrorPlotModel.Series.Add(targetLine);
            }

            ErrorPlotModel.InvalidatePlot(true);
        }

        private void UpdatePlot()
        {
            PlotModel.Series.Clear();
            
            // Clear sticky annotation when updating plot
            if (_currentAnnotation != null)
            {
                PlotModel.Annotations.Remove(_currentAnnotation);
                _currentAnnotation = null;
            }

            var pointsSeries = new ScatterSeries
            {
                Title = "Original Data",
                MarkerType = MarkerType.Circle,
                MarkerSize = 8,
                MarkerFill = OxyColors.Red,
                MarkerStroke = OxyColors.White,
                MarkerStrokeThickness = 2,
                TrackerFormatString = "Original Data\nX: {2:0.####}\nY: {4:0.####}"
            };

            foreach (var point in DataPoints)
            {
                pointsSeries.Points.Add(new ScatterPoint(point.X, point.Y));
            }

            PlotModel.Series.Add(pointsSeries);

            var colors = new[] { OxyColors.Cyan, OxyColors.Lime, OxyColors.Yellow, 
                               OxyColors.Magenta, OxyColors.Orange };
            int colorIndex = 0;

            // ✅ Prikaži SAMO vidljive rezultate
            foreach (var result in Results.Where(r => r.IsVisible))
            {
                var lineSeries = new LineSeries
                {
                    Title = $"{result.AlgorithmName} (RMSE: {result.RMSError:F4})",
                    Color = colors[colorIndex % colors.Length],
                    StrokeThickness = 2.5,
                    TrackerFormatString = $"{result.AlgorithmName}\n━━━━━━━━━━━━\nX: {{2:0.####}}\nY: {{4:0.####}}\nRMSE: {result.RMSError:F4}",
                    CanTrackerInterpolatePoints = false
                };

                foreach (var point in result.InterpolatedPoints)
                {
                    lineSeries.Points.Add(new OxyPlot.DataPoint(point.X, point.Y));
                }

                PlotModel.Series.Add(lineSeries);
                colorIndex++;
            }

            PlotModel.InvalidatePlot(true);
        }

        [RelayCommand]
        private void LoadAudioFile()
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Audio Files|*.wav;*.mp3;*.flac;*.aiff|All Files|*.*",
                    Title = "Select Audio File"
                };

                if (dialog.ShowDialog() == true)
                {
                    var (samples, sampleRate, channels) = _audioService.LoadAudioFile(dialog.FileName);
                    _audioSamples = samples;
                    _audioSampleRate = sampleRate;
                    _audioChannels = channels;
                    IsAudioLoaded = true;
                    AudioFileName = Path.GetFileName(dialog.FileName);
                    AudioSampleCount = samples.Length;
                    StatusMessage = $"Loaded: {AudioFileName} ({samples.Length} samples, {sampleRate}Hz, {channels}ch)";
                    ErrorMessage = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading audio: {ex.Message}";
                IsAudioLoaded = false;
            }
        }

        [RelayCommand]
        private void ProcessAudioNoise()
        {
            try
            {
                if (_audioSamples == null)
                {
                    ErrorMessage = "No audio loaded!";
                    return;
                }

                var algorithm = CreateAlgorithm(SelectedAlgorithm);
                StatusMessage = "Processing audio - removing noise...";
                var stopwatch = Stopwatch.StartNew();

                int originalLength = _audioSamples.Length;
                var processed = _audioService.RemoveNoiseInterpolation(_audioSamples, algorithm, NoiseThreshold);
                stopwatch.Stop();

                _audioSamples = processed;
                StatusMessage = $"Noise removal complete in {stopwatch.ElapsedMilliseconds}ms. Processed {processed.Length} samples";
                ErrorMessage = string.Empty;
                
                MessageBox.Show(
                    $"Audio Processing Complete!\n\n" +
                    $"Time: {stopwatch.ElapsedMilliseconds}ms\n" +
                    $"Original samples: {originalLength:N0}\n" +
                    $"Processed samples: {processed.Length:N0}\n" +
                    $"Algorithm: {SelectedAlgorithm}\n" +
                    $"Threshold: {NoiseThreshold}σ",
                    "Processing Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error processing audio: {ex.Message}";
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void SaveAudioFile()
        {
            try
            {
                if (_audioSamples == null)
                {
                    ErrorMessage = "No audio to save!";
                    return;
                }

                var dialog = new SaveFileDialog
                {
                    Filter = "WAV Files|*.wav",
                    Title = "Save Processed Audio",
                    FileName = "processed_audio.wav"
                };

                if (dialog.ShowDialog() == true)
                {
                    _audioService.SaveAudioFile(dialog.FileName, _audioSamples, _audioSampleRate, _audioChannels);
                    StatusMessage = $"Saved: {Path.GetFileName(dialog.FileName)}";
                    ErrorMessage = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving audio: {ex.Message}";
            }
        }

        [RelayCommand]
        private void ExportData()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "CSV Files|*.csv",
                    Title = "Export Data",
                    FileName = "interpolation_data.csv"
                };

                if (dialog.ShowDialog() == true)
                {
                    using var writer = new StreamWriter(dialog.FileName);
                    writer.WriteLine("X,Y,Algorithm");
                    
                    foreach (var result in Results)
                    {
                        foreach (var point in result.InterpolatedPoints)
                        {
                            writer.WriteLine($"{point.X},{point.Y},{result.AlgorithmName}");
                        }
                    }

                    StatusMessage = $"Exported to: {Path.GetFileName(dialog.FileName)}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Export error: {ex.Message}";
            }
        }

        [RelayCommand]
        private void EvaluateAtPoint()
        {
            try
            {
                if (Results.Count == 0)
                {
                    ErrorMessage = "Please interpolate data first!";
                    return;
                }
                
                double xValue;
                try
                {
                    xValue = ParseFlexibleDouble(EvaluateXText);
                }
                catch (FormatException ex)
                {
                    ErrorMessage = $"Invalid X value: {ex.Message}";
                    return;
                }
                
                ErrorMessage = string.Empty;
                EvaluatedResults.Clear();
                
                double[] xPoints = DataPoints.Select(p => p.X).ToArray();
                double[] yPoints = DataPoints.Select(p => p.Y).ToArray();
                
                foreach (var algoName in AvailableAlgorithms)
                {
                    try
                    {
                        var algorithm = CreateAlgorithm(algoName);
                        algorithm.SetData(xPoints, yPoints);
                        double yValue = algorithm.Interpolate(xValue);
                        
                        EvaluatedResults.Add(new EvaluationResult
                        {
                            AlgorithmName = algoName,
                            XValue = xValue,
                            YValue = yValue
                        });
                    }
                    catch (Exception ex)
                    {
                        // Skip algoritam ako ne može interpolirati
                        Console.WriteLine($"[ERROR] {algoName} evaluation failed: {ex.Message}");
                    }
                }
                
                StatusMessage = $"Evaluated at X = {xValue:F2} for {EvaluatedResults.Count} algorithms";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Evaluation error: {ex.Message}";
            }
        }
        
        [RelayCommand]
        private void ExportPlot()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "PNG Files|*.png",
                    Title = "Export Plot",
                    FileName = "interpolation_plot.png"
                };

                if (dialog.ShowDialog() == true)
                {
                    using var stream = File.Create(dialog.FileName);
                    var exporter = new PngExporter { Width = 1920, Height = 1080 };
                    exporter.Export(PlotModel, stream);
                    StatusMessage = $"Plot exported: {Path.GetFileName(dialog.FileName)}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Export error: {ex.Message}";
            }
        }

        private AppIInterpolationAlgorithm CreateAlgorithm(string name)
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
        
        /// <summary>
        /// Generiše polinomsku jednačinu u čitljivom formatu
        /// </summary>
        private string GeneratePolynomialEquation(AppIInterpolationAlgorithm algorithm, double[] xPoints, double[] yPoints)
        {
            try
            {
                int n = xPoints.Length;
                const int MAX_TERMS_TO_SHOW = 4; // Prikaži max 4 člana + "..."
                bool showPartial = n > 6;
                
                // Izračunaj koeficijente polinoma evaluacijom na različitim tačkama
                // Koristimo Vandermonde pristup za aproksimaciju
                double xMin = xPoints.Min();
                double xMax = xPoints.Max();
                int numSamples = 50;
                double step = (xMax - xMin) / (numSamples - 1);
                
                // Kreiraj sistem jednačina za fitting polinoma
                var matrix = new double[n, n];
                var rhs = new double[n];
                
                for (int i = 0; i < n; i++)
                {
                    double x = xPoints[i];
                    rhs[i] = yPoints[i];
                    for (int j = 0; j < n; j++)
                    {
                        matrix[i, j] = Math.Pow(x, j);
                    }
                }
                
                // Rješi sistem (jednostavno Gaussian elimination)
                var coefficients = SolveLinearSystem(matrix, rhs, n);
                
                // Formatiraj jednačinu
                var terms = new List<string>();
                int termsAdded = 0;
                
                for (int i = n - 1; i >= 0; i--)
                {
                    double coef = coefficients[i];
                    if (Math.Abs(coef) < 1e-10) continue;
                    
                    // Ako prikazujemo samo djelimično i dostigli smo MAX_TERMS_TO_SHOW
                    if (showPartial && termsAdded >= MAX_TERMS_TO_SHOW)
                    {
                        terms.Add("+ ...");
                        break;
                    }
                    
                    string sign = coef >= 0 ? "+" : "-";
                    double absCoef = Math.Abs(coef);
                    
                    if (i == 0)
                    {
                        terms.Add($"{sign} {absCoef:F3}");
                    }
                    else if (i == 1)
                    {
                        if (Math.Abs(absCoef - 1.0) < 1e-10)
                            terms.Add($"{sign} x");
                        else
                            terms.Add($"{sign} {absCoef:F3}x");
                    }
                    else
                    {
                        if (Math.Abs(absCoef - 1.0) < 1e-10)
                            terms.Add($"{sign} x^{i}");
                        else
                            terms.Add($"{sign} {absCoef:F3}x^{i}");
                    }
                    
                    termsAdded++;
                }
                
                if (terms.Count == 0)
                    return "P(x) = 0";
                    
                string equation = "P(x) = " + string.Join(" ", terms);
                
                // Ukloni leading + znak
                equation = equation.Replace("= +", "=").Replace("= -", "= -");
                
                // Dodaj napomenu ako je skraćeno
                if (showPartial)
                {
                    equation += $" [deg {n-1}]";
                }
                
                return equation;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to generate equation: {ex.Message}");
                return "P(x) = [Unable to generate equation]";
            }
        }
        
        /// <summary>
        /// Rješava linearni sistem jednačina Ax = b koristeći Gaussian elimination
        /// </summary>
        private double[] SolveLinearSystem(double[,] A, double[] b, int n)
        {
            // Kreiraj augmented matrix
            var aug = new double[n, n + 1];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    aug[i, j] = A[i, j];
                }
                aug[i, n] = b[i];
            }
            
            // Forward elimination
            for (int i = 0; i < n; i++)
            {
                // Pronađi pivot
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(aug[k, i]) > Math.Abs(aug[maxRow, i]))
                        maxRow = k;
                }
                
                // Swap rows
                for (int k = i; k < n + 1; k++)
                {
                    double tmp = aug[maxRow, k];
                    aug[maxRow, k] = aug[i, k];
                    aug[i, k] = tmp;
                }
                
                // Eliminacija
                for (int k = i + 1; k < n; k++)
                {
                    double factor = aug[k, i] / aug[i, i];
                    for (int j = i; j < n + 1; j++)
                    {
                        aug[k, j] -= factor * aug[i, j];
                    }
                }
            }
            
            // Back substitution
            var x = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                x[i] = aug[i, n];
                for (int j = i + 1; j < n; j++)
                {
                    x[i] -= aug[i, j] * x[j];
                }
                x[i] /= aug[i, i];
            }
            
            return x;
        }
    }

    public class DegreeErrorResult
    {
        public int Degree { get; set; }
        public double MaxError { get; set; }
        public double AvgError { get; set; }
    }
    
    public class EvaluationResult
    {
        public string AlgorithmName { get; set; } = string.Empty;
        public double XValue { get; set; }
        public double YValue { get; set; }
        public string FormattedResult => $"Y = {YValue:F6}";
    }
    
    // Sticky Tooltip Handlers
    partial class MainViewModel
    {
        private PointAnnotation? _currentAnnotation;
        private PointAnnotation? _currentFunctionAnnotation;
        
        private void OnPlotModelMouseDown(object? sender, OxyMouseDownEventArgs e)
        {
            if (e.ChangedButton != OxyMouseButton.Left)
                return;
                
            // Remove previous annotation if exists
            if (_currentAnnotation != null)
            {
                PlotModel.Annotations.Remove(_currentAnnotation);
                _currentAnnotation = null;
                PlotModel.InvalidatePlot(false);
                // Ne dodajemo novu annotation - samo brišemo postojeću
                return;
            }
            
            // Pronađi najbližu seriju i hit
            Series? closestSeries = null;
            TrackerHitResult? closestHit = null;
            double minDistance = double.MaxValue;
            
            // Provjeri sve serije (LineSeries i ScatterSeries)
            foreach (var series in PlotModel.Series)
            {
                TrackerHitResult? hit = null;
                
                if (series is LineSeries lineSeries)
                {
                    hit = lineSeries.GetNearestPoint(e.Position, true);
                }
                else if (series is ScatterSeries scatterSeries)
                {
                    hit = scatterSeries.GetNearestPoint(e.Position, true);
                }
                
                if (hit != null)
                {
                    double distance = Math.Sqrt(
                        Math.Pow(hit.Position.X - e.Position.X, 2) + 
                        Math.Pow(hit.Position.Y - e.Position.Y, 2));
                    
                    if (distance < minDistance && distance < 30) // max 30 pixel distance
                    {
                        minDistance = distance;
                        closestSeries = series;
                        closestHit = hit;
                    }
                }
            }
            
            if (closestSeries != null && closestHit != null)
            {
                DataPoint dp;
                
                if (closestSeries is LineSeries ls)
                {
                    dp = ls.InverseTransform(closestHit.Position);
                }
                else if (closestSeries is ScatterSeries ss)
                {
                    dp = ss.InverseTransform(closestHit.Position);
                }
                else
                {
                    return;
                }
                
                // Izvuci podatke iz Result objekta ako postoji
                string displayInfo = closestSeries.Title ?? "Unknown";
                
                // Create sticky annotation
                _currentAnnotation = new PointAnnotation
                {
                    X = dp.X,
                    Y = dp.Y,
                    Text = $"{displayInfo}\n━━━━━━━━━━━━\nX: {dp.X:F4}\nY: {dp.Y:F4}",
                    TextColor = OxyColors.White,
                    Fill = OxyColor.FromArgb(230, 63, 81, 181),
                    Stroke = OxyColors.White,
                    StrokeThickness = 2.5,
                    Size = 12,
                    TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left,
                    TextVerticalAlignment = OxyPlot.VerticalAlignment.Bottom,
                    FontSize = 13,
                    FontWeight = 700
                };
                
                PlotModel.Annotations.Add(_currentAnnotation);
                PlotModel.InvalidatePlot(false);
            }
        }
        
        private void OnFunctionPlotModelMouseDown(object? sender, OxyMouseDownEventArgs e)
        {
            if (e.ChangedButton != OxyMouseButton.Left)
                return;
                
            // Remove previous annotation if exists
            if (_currentFunctionAnnotation != null)
            {
                FunctionPlotModel.Annotations.Remove(_currentFunctionAnnotation);
                _currentFunctionAnnotation = null;
                FunctionPlotModel.InvalidatePlot(false);
                // Ne dodajemo novu annotation - samo brišemo postojeću
                return;
            }
            
            // Pronađi najbližu seriju i hit
            Series? closestSeries = null;
            TrackerHitResult? closestHit = null;
            double minDistance = double.MaxValue;
            
            // Provjeri sve serije (LineSeries i ScatterSeries)
            foreach (var series in FunctionPlotModel.Series)
            {
                TrackerHitResult? hit = null;
                
                if (series is LineSeries lineSeries)
                {
                    hit = lineSeries.GetNearestPoint(e.Position, true);
                }
                else if (series is ScatterSeries scatterSeries)
                {
                    hit = scatterSeries.GetNearestPoint(e.Position, true);
                }
                
                if (hit != null)
                {
                    double distance = Math.Sqrt(
                        Math.Pow(hit.Position.X - e.Position.X, 2) + 
                        Math.Pow(hit.Position.Y - e.Position.Y, 2));
                    
                    if (distance < minDistance && distance < 30) // max 30 pixel distance
                    {
                        minDistance = distance;
                        closestSeries = series;
                        closestHit = hit;
                    }
                }
            }
            
            if (closestSeries != null && closestHit != null)
            {
                DataPoint dp;
                
                if (closestSeries is LineSeries ls)
                {
                    dp = ls.InverseTransform(closestHit.Position);
                }
                else if (closestSeries is ScatterSeries ss)
                {
                    dp = ss.InverseTransform(closestHit.Position);
                }
                else
                {
                    return;
                }
                
                // Izvuci podatke iz Result objekta ako postoji
                string displayInfo = closestSeries.Title ?? "Unknown";
                
                // Create sticky annotation
                _currentFunctionAnnotation = new PointAnnotation
                {
                    X = dp.X,
                    Y = dp.Y,
                    Text = $"{displayInfo}\n━━━━━━━━━━━━\nX: {dp.X:F4}\nY: {dp.Y:F4}",
                    TextColor = OxyColors.White,
                    Fill = OxyColor.FromArgb(230, 33, 150, 243),
                    Stroke = OxyColors.White,
                    StrokeThickness = 2.5,
                    Size = 12,
                    TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left,
                    TextVerticalAlignment = OxyPlot.VerticalAlignment.Bottom,
                    FontSize = 13,
                    FontWeight = 700
                };
                
                FunctionPlotModel.Annotations.Add(_currentFunctionAnnotation);
                FunctionPlotModel.InvalidatePlot(false);
            }
        }
    }
}
