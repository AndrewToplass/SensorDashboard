using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace SensorDashboard;

public class DataGridCellColorMultiConverter : AvaloniaObject, IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 3)
        {
            return AvaloniaProperty.UnsetValue;
        }

        if (values[0] is not double min ||
            values[1] is not double max ||
            values[2] is not string content)
        {
            return AvaloniaProperty.UnsetValue;
        }

        if (double.TryParse(content, out var result))
        {
            return result switch
            {
                _ when result > max => Brushes.Red,
                _ when result < min => Brushes.Blue,
                _ => Brushes.Green
            };
        }

        return AvaloniaProperty.UnsetValue;
    }
}
