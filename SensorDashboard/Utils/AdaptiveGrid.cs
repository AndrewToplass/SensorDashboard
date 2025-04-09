using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;

namespace SensorDashboard.Utils;

/// <summary>
/// Grid that can automatically set number of row definitions and automatically
/// position child controls based on explicitly defined column definitions.
/// </summary>
public class AdaptiveGrid : Grid
{
    public static readonly StyledProperty<GridLength> RowHeightProperty =
        AvaloniaProperty.Register<AdaptiveGrid, GridLength>(
            nameof(RowHeight), GridLength.Auto);

    public static readonly StyledProperty<ColumnDefinitions> ColumnDefinitionsProperty =
        AvaloniaProperty.Register<AdaptiveGrid, ColumnDefinitions>(
            nameof(ColumnDefinitions), []);

    static AdaptiveGrid()
    {
        ColumnDefinitionsProperty.Changed.AddClassHandler<AdaptiveGrid>(OnColumnsDefinitionsPropertyChanged);
    }

    /// <summary>
    /// Height of each row, can be any single grid length (e.g. auto, px, *).
    /// </summary>
    public GridLength RowHeight
    {
        get => GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    /// <summary>
    /// Set Grid column definitions, rows definitions will be generated based on
    /// number of children and number of columns.
    /// </summary>
    public new ColumnDefinitions ColumnDefinitions
    {
        get => GetValue(ColumnDefinitionsProperty);
        set => SetValue(ColumnDefinitionsProperty, value);
    }

    /// <summary>
    /// The row definitions currently in use.
    /// </summary>
    public new IReadOnlyList<RowDefinition> RowDefinitions => base.RowDefinitions;

    protected override void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.ChildrenChanged(sender, e);
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                ApplyRowDefinitions();
                ApplyGridPositionsToChildren(e.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Remove:
                ApplyRowDefinitions();
                ApplyGridPositionsToChildren(e.OldStartingIndex);
                break;

            case NotifyCollectionChangedAction.Move:
                var start = Math.Min(e.OldStartingIndex, e.NewStartingIndex);
                var end = Math.Max(e.OldStartingIndex, e.NewStartingIndex) + 1;
                ApplyGridPositionsToChildren(start, end);
                break;

            case NotifyCollectionChangedAction.Replace:
                var index = e.NewStartingIndex;
                var row = index / ColumnDefinitions.Count;
                var column = index % ColumnDefinitions.Count;
                Children[index][RowProperty] = row;
                Children[index][ColumnProperty] = column;
                break;

            case NotifyCollectionChangedAction.Reset:
            default:
                break;
        }
    }

    private void ApplyGridPositionsToChildren(int startIndex = 0, int endIndex = int.MaxValue)
    {
        var row = startIndex / Math.Max(ColumnDefinitions.Count, 1);
        var column = startIndex % Math.Max(ColumnDefinitions.Count, 1);
        for (var i = startIndex;
             row < RowDefinitions.Count && i < Children.Count && i < endIndex;
             i++, column++)
        {
            if (column >= ColumnDefinitions.Count)
            {
                column = 0;
                row++;
            }

            var child = Children[i];
            child[ColumnProperty] = column;
            child[RowProperty] = row;
        }
    }

    private void ApplyRowDefinitions()
    {
        var desiredRows = Children.Count / Math.Max(ColumnDefinitions.Count, 1)
                          + Math.Min(Children.Count % Math.Max(ColumnDefinitions.Count, 1), 1);

        if (RowDefinitions.Count < desiredRows)
        {
            for (var i = RowDefinitions.Count; i < desiredRows; i++)
            {
                base.RowDefinitions.Add(new RowDefinition
                {
                    // Bind row definition height to declared row height
                    // property.
                    [!RowDefinition.HeightProperty] = this[!RowHeightProperty]
                });
            }

            return;
        }

        if (RowDefinitions.Count > desiredRows)
        {
            for (var i = RowDefinitions.Count - 1; i >= desiredRows && i >= 1; i--)
            {
                base.RowDefinitions.RemoveAt(i);
            }
        }
    }

    private static void OnColumnsDefinitionsPropertyChanged(AdaptiveGrid grid, AvaloniaPropertyChangedEventArgs e)
    {
        // Need to set ColumnDefinitions in base class as this class exposes a
        // new readonly view of the generated column definitions.
        (grid as Grid).ColumnDefinitions = e.GetNewValue<ColumnDefinitions>();

        // Generate row definitions based on the new columns.
        grid.ApplyRowDefinitions();

        // Setting column definitions doesn't mark existing definitions as
        // dirty, so needs to be manually set using an internal property.
        typeof(Grid)
            .GetProperty("ColumnDefinitionsDirty", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(grid, true);

        grid.ApplyGridPositionsToChildren();
    }
}
