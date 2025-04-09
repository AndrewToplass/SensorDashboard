using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace SensorDashboard.Utils;

public static class Extensions
{
    /// <summary>
    /// Blend two colours together in the HSL colour space, optionally
    /// specifying the blend amount.
    /// </summary>
    /// <param name="thisCol">The first colour to blend.</param>
    /// <param name="otherCol">The second colour to blend.</param>
    /// <param name="t">The amount to blend the two colours from 0 to 1. 0 being
    /// full first, 1 being full second, 0.5 being half mix between the two.
    /// </param>
    /// <returns>Blended colour from the two original colours.</returns>
    public static Color Blend(this Color thisCol, Color otherCol, double t = 0.5)
    {
        var first = new HslColor(thisCol);
        var second = new HslColor(otherCol);

        return HslColor
            .FromAhsl(
                Lerp(first.A, second.A, t),
                Lerp(first.H, second.H, t),
                Lerp(first.S, second.S, t),
                Lerp(first.L, second.L, t))
            .ToRgb();

        static double Lerp(double a, double b, double t) => a + t * (b - a);
    }

    /// <summary>
    /// Tries to the specified resource, but limits search to specified type.
    /// Original method from: <see cref="ResourceNodeExtensions"/>
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="key">The resource key.</param>
    /// <param name="theme">Theme used to select theme dictionary.</param>
    /// <param name="value">On return, contains the resource if found, otherwise null.</param>
    /// <typeparam name="T">The type of the resource to find.</typeparam>
    /// <returns>True if the resource was found; otherwise false.</returns>
    public static bool TryFindResource<T>(
        this IResourceHost control,
        object key,
        ThemeVariant? theme,
        [NotNullWhen(true)] out T? value)
    {
        if (control.TryFindResource(key, theme, out var objectValue))
        {
            if (objectValue is T tValue)
            {
                value = tValue;
                return true;
            }
        }

        value = default;
        return false;
    }
}
