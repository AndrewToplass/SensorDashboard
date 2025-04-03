using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;

namespace SensorDashboard.Models;

public partial class DataProcessor : ObservableObject
{
    public static DataProcessor Instance { get; } = new();

    private DataProcessor()
    {
    }

    [ObservableProperty] private SortedObservableCollection<SensorData> _datasets = [];

    public double Average(SensorData data) =>
        Enumerable.Range(0, data.Rows)
            .SelectMany(data.GetRow)
            .Average();

    public double AverageOfRow(SensorData data, int row) => data.GetRow(row).Average();
}
