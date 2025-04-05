using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SensorDashboard.Models;

public partial class DataProcessor : ObservableObject
{
    public static DataProcessor Instance { get; } = new();

    private DataProcessor()
    {
    }

    [ObservableProperty] private SortedCollection<SensorData> _datasets = new(new SensorDataTitleComparer());

    public double AverageOfDataset(SensorData data)
    {
        if (data.Rows == 0)
        {
            return 0;
        }

        return Enumerable.Range(0, data.Rows)
            .SelectMany(data.GetRow)
            .Average();
    }

    public double AverageOfDatasetRow(SensorData data, int row) => data.GetRow(row).Average();

    public async Task<SensorData> OpenDatasetAsync(string filePath)
    {
        var dataset = await SensorData.FromFileAsync(filePath);
        Datasets.Add(dataset);
        return dataset;
    }

    public async Task SaveDatasetAsync(SensorData dataset, string filePath) => await dataset.SaveToFileAsync(filePath);

    public void CloseDataset(SensorData dataset) => Datasets.Remove(dataset);

    public SensorData NewDataset(string title)
    {
        var dataset = new SensorData { Title = title, HasUnsavedChanges = false };
        Datasets.Add(dataset);
        return dataset;
    }

    public SensorData OpenTestDataset()
    {
        var dataset = SensorData.FromTest();
        Datasets.Add(dataset);
        return dataset;
    }
}
