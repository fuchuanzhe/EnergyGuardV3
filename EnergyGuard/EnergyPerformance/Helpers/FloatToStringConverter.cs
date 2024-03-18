using System.Globalization;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;
namespace EnergyPerformance.Helpers;

/// <summary>
/// Converter class to convert a float value to a string so it can be displayed in the UI's home page.
/// </summary>
public class FloatToStringConverter : IValueConverter
{
    public FloatToStringConverter()
    {
    }

    /// <summary>
    /// Converts a double to a string with 2 decimal places. Used for the Power Usage text in the home page.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is float val)
        {
            var roundedValue = val.ToString("0.00");
            return roundedValue;
        }
        return "0.00";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // convert back method should not be called for a 1-way binding
        throw new NotImplementedException("ExceptionConvertBackMethodNotImplemented");
    }
}
