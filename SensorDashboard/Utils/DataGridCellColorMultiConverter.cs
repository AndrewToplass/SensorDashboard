using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;

namespace SensorDashboard.Utils;

public class DataGridCellColorMultiConverter : AvaloniaObject, IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 5 ||
            values[0] is not double min ||
            values[1] is not double max ||
            values[2] is not string content ||
            values[3] is not ThemeVariant theme ||
            values[4] is not bool gradientMode)
        {
            return Brushes.Transparent;
        }

        if (double.TryParse(content, out var cellValue))
        {
            return gradientMode
                ? CalculateBlendedColour(cellValue, min, max, theme)
                : CalculateNonBlendedColour(cellValue, min, max, theme);
        }

        return Brushes.Transparent;
    }

    private static IBrush CalculateNonBlendedColour(double value, double min, double max, ThemeVariant? theme = null)
    {
        if (Application.Current is null ||
            !Application.Current.TryFindResource<IBrush>("DataGridCellGoodBackgroundBrush", theme, out var brushGood) ||
            !Application.Current.TryFindResource<IBrush>("DataGridCellHighBackgroundBrush", theme, out var brushHigh) ||
            !Application.Current.TryFindResource<IBrush>("DataGridCellLowBackgroundBrush", theme, out var brushLow))
        {
            return Brushes.Transparent;
        }

        return value switch
        {
            _ when value < min => brushLow,
            _ when value > max => brushHigh,
            _ => brushGood
        };
    }

    private static IBrush CalculateBlendedColour(double value, double min, double max, ThemeVariant? theme = null)
    {
        if (Application.Current is null ||
            !Application.Current.TryFindResource<Color>("DataGridCellGoodBackground", theme, out var colorGood) ||
            !Application.Current.TryFindResource<Color>("DataGridCellHighBackground", theme, out var colorHigh) ||
            !Application.Current.TryFindResource<Color>("DataGridCellLowBackground", theme, out var colorLow))
        {
            return Brushes.Transparent;
        }

        var t = Math.Clamp((value - min) / (max - min), 0, 1) * 2 - 1;

        return new SolidColorBrush(
            t switch
            {
                < 0 => colorGood.Blend(colorLow, -t),
                > 0 => colorGood.Blend(colorHigh, t),
                _ => colorGood
            });
    }
}
