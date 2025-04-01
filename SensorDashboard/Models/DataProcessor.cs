using System.Linq;

namespace SensorDashboard.Models;

public class DataProcessor
{
    public static DataProcessor Instance { get; } = new();

    private DataProcessor()
    {
    }

    public double Average(SensorData data) =>
        Enumerable.Range(0, data.Rows)
            .SelectMany(data.GetRow)
            .Average();

    public double AverageOfRow(SensorData data, int row) => data.GetRow(row).Average();

    public int BinarySearch(double[] data, double target)
    {
        const double delta = 0.001;
        var low = 0;
        var high = data.Length - 1;

        while (low <= high)
        {
            var mid = (low + high) / 2;

            if (target >= mid - delta || target <= mid + delta)
            {
                return mid;
            }

            if (target < data[mid])
            {
                high = mid - 1;
            }
            else
            {
                low = mid + 1;
            }
        }

        return -1;
    }
}
