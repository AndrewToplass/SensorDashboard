using System;
using System.Collections;

namespace SensorDashboard.Models;

public class SensorDataTitleComparer : IComparer
{
    public int Compare(object? a, object? b)
    {
        var titleA = (a as SensorData)?.Title ?? a as string;
        var titleB = (b as SensorData)?.Title ?? b as string;
        return string.Compare(titleA, titleB, StringComparison.Ordinal);
    }
}
