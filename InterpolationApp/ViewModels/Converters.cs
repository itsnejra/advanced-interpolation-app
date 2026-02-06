using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace InterpolationApp.ViewModels
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter koji prihvata i zarez (,) i tačku (.) kao decimalni separator
    /// </summary>
    public class FlexibleDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Konverzija iz double u string za prikaz u TextBox-u
            if (value is double doubleValue)
            {
                // Koristi kulturu sa zarezom kao separatorom za prikaz
                return doubleValue.ToString(CultureInfo.CurrentCulture);
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Konverzija iz stringa u double pri unosu
            if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
            {
                // Prvo pokušaj sa trenutnom kulturom
                if (double.TryParse(stringValue, NumberStyles.Any, CultureInfo.CurrentCulture, out double result))
                {
                    return result;
                }

                // Ako ne uspe, pokušaj sa invariant kulturom (tačka kao separator)
                if (double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                {
                    return result;
                }

                // Pokušaj zamijeniti separator i ponovo parsirati
                string normalized = stringValue.Replace(',', '.').Replace(" ", "");
                if (double.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                {
                    return result;
                }

                // Ako ni to ne uspe, vrati originalnu vrednost ili 0
                return 0.0;
            }

            return 0.0;
        }
    }
}
