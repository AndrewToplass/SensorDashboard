using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace SensorDashboard.Utils;

public class DataGridCellPaddingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double fontSize) return AvaloniaProperty.UnsetValue;
        if (targetType != typeof(Thickness)) return AvaloniaProperty.UnsetValue;

        var paddingBase = new Thickness(12, 8);

        // return AvaloniaProperty.UnsetValue;

        return new Thickness(
            paddingBase.Left,
            Math.Clamp(paddingBase.Top * (fontSize - 10) / 4, 0, paddingBase.Top)
        );
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
