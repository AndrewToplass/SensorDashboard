using System.IO;
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

    public async Task<SensorData> OpenDatasetAsync(Stream stream, FileFormat fileFormat)
    {
        var dataset = await SensorData.FromStreamAsync(stream, fileFormat);
        Datasets.Add(dataset);
        return dataset;
    }

    public async Task SaveDatasetAsync(SensorData dataset, Stream stream, FileFormat fileFormat) =>
        await dataset.SaveToStreamAsync(stream, fileFormat);

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
