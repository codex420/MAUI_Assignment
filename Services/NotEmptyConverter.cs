using System.Globalization;

namespace MAUI_Assignment.Services;

/// <summary>Returns true when the bound string is non-empty.</summary>
public class NotEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrWhiteSpace(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
