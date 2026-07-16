using System.Globalization;

namespace MAUI_Assignment.Services;

/// <summary>Maps a bool to a <see cref="StackOrientation"/> (true → Vertical).</summary>
public class BoolToOrientationConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? StackOrientation.Vertical : StackOrientation.Horizontal;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns the gap margin for the second item in a reflowing two-item row.
/// Stacked (true) → top gap; side-by-side (false) → left gap.
/// </summary>
public class RowGapConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? new Thickness(0, 18, 0, 0) : new Thickness(18, 0, 0, 0);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Inverts a boolean.</summary>
public class InvertBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not true;
}

/// <summary>
/// Returns FlexLayout basis-friendly widths. Given a bool "stacked",
/// returns a proportional flex basis string: full width when stacked,
/// otherwise the supplied ConverterParameter (e.g. "70%").
/// </summary>
public class StackedToBasisConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool stacked = value is true;
        // When stacked, take the full row so the next item wraps beneath it.
        if (stacked)
            return new Microsoft.Maui.Layouts.FlexBasis(1f, isRelative: true);

        var pct = (parameter as string) ?? "50%";
        pct = pct.Replace("%", string.Empty);
        if (float.TryParse(pct, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
            return new Microsoft.Maui.Layouts.FlexBasis(f / 100f, isRelative: true);

        return Microsoft.Maui.Layouts.FlexBasis.Auto;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
