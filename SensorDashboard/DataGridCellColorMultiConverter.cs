using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;

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
            values[2] is not string content ||
            values[3] is not TreeDataGridCell cell)
        {
            return AvaloniaProperty.UnsetValue;
        }

        if (double.TryParse(content, out var result))
        {
            // Apply dynamic resource that can adapt to theme changes.
            cell[!TemplatedControl.BackgroundProperty] = result switch
            {
                _ when result > max => new DynamicResourceExtension("DataGridCellHighBackgroundBrush"),
                _ when result < min => new DynamicResourceExtension("DataGridCellLowBackgroundBrush"),
                _ => new DynamicResourceExtension("DataGridCellGoodBackgroundBrush")
            };
        }

        return AvaloniaProperty.UnsetValue;
    }
}
