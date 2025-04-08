using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;

namespace SensorDashboard.Utils;

public class DataGridCellColorMultiConverter : AvaloniaObject, IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 4 ||
            values[0] is not double min ||
            values[1] is not double max ||
            values[2] is not string content ||
            values[3] is not ThemeVariant theme)
        {
            return Brushes.Transparent;
        }

        if (double.TryParse(content, out var result))
        {
            var key = result switch
            {
                _ when result > max => "DataGridCellHighBackgroundBrush",
                _ when result < min => "DataGridCellLowBackgroundBrush",
                _ => "DataGridCellGoodBackgroundBrush"
            };

            if (Application.Current?.TryFindResource(key, theme, out var value) ?? false)
            {
                return value;
            }
        }

        return Brushes.Transparent;
    }
}
