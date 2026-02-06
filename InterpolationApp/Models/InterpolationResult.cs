using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace InterpolationApp.Models
{
    /// <summary>
    /// Contains results from interpolation operation
    /// </summary>
    public class InterpolationResult : INotifyPropertyChanged
    {
        public string AlgorithmName { get; set; } = string.Empty;
        public List<DataPoint> OriginalPoints { get; set; } = new();
        public List<DataPoint> InterpolatedPoints { get; set; } = new();
        public double RMSError { get; set; }
        public double MaxError { get; set; }
        public TimeSpan ComputationTime { get; set; }
        public Dictionary<string, object> AdditionalInfo { get; set; } = new();
        
        // Nova polja za napredne funkcionalnosti
        private string _polynomialEquation = string.Empty;
        public string PolynomialEquation 
        { 
            get => _polynomialEquation;
            set
            {
                if (_polynomialEquation != value)
                {
                    _polynomialEquation = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private bool _isVisible = true;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"{AlgorithmName}: RMSE={RMSError:F6}, MaxErr={MaxError:F6}, Time={ComputationTime.TotalMilliseconds:F2}ms";
        }
    }
}
