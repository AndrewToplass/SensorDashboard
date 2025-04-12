using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using SensorDashboard.Models;

namespace SensorDashboard.ViewModels;

public partial class FileTabViewModel : ViewModelBase
{
    public FileTabViewModel()
    {
        var selection = new TreeDataGridRowSelectionModel<double[]>(DataGridSource)
        {
            SingleSelect = true
        };
        DataGridSource.Selection = selection;
        selection.PropertyChanged += TreeDataGridRowSelectionModel_PropertyChanged;
    }

    [ObservableProperty] private double _thresholdMinimum = 15;

    [ObservableProperty] private double _thresholdMaximum = 35;

    [ObservableProperty] private bool _useThresholdGradient = false;

    [ObservableProperty] private FlatTreeDataGridSource<double[]> _dataGridSource = new([]);

    [ObservableProperty] private double _dataGridFontSize = 14;

    [ObservableProperty] private double _averageTotal;

    [ObservableProperty] private double? _averageRow;

    [ObservableProperty] private IStorageFile? _file;

    // Set by required property SensorData.
    private SensorData _sensorData = new() { Title = "" };

    public required SensorData SensorData
    {
        get => _sensorData;
        set
        {
            SensorData.PropertyChanged -= SensorData_PropertyChanged;
            SetProperty(ref _sensorData, value);
            SensorData.PropertyChanged += SensorData_PropertyChanged;
            ApplyDataGridColumns();

            DataGridSource.Items =
            [
                ..Enumerable
                    .Range(0, SensorData.Rows)
                    .Select(i => SensorData.GetRow(i).ToArray())
            ];
            AverageTotal = DataProcessor.Instance.AverageOfDataset(SensorData);
        }
    }

    public override string ToString()
    {
        return SensorData.Title;
    }

    public void OpenSampleData()
    {
        SensorData = DataProcessor.Instance.OpenTestDataset();
    }

    public async Task OpenFile(IStorageFile file)
    {
        File = file;
        await using var stream = await file.OpenReadAsync();

        var bytes = new byte[stream.Length];
        if (await stream.ReadAsync(bytes) != bytes.Length)
        {
            throw new IOException("Unable to read file.");
        }

        var memoryStream = new MemoryStream(bytes);
        SensorData = await DataProcessor.Instance.OpenDatasetAsync(memoryStream, file.Name);
    }

    public async Task<bool> SaveFile()
    {
        if (File is null)
        {
            return false;
        }

        await using var buffer = new MemoryStream();
        await DataProcessor.Instance.SaveDatasetAsync(SensorData, buffer, File.Name);

        await using var stream = await File.OpenWriteAsync();
        await stream.WriteAsync(buffer.ToArray());

        return true;
    }

    public async Task SaveFile(IStorageFile file)
    {
        File = file;
        await SaveFile();
    }

    private void ApplyDataGridColumns()
    {
        DataGridSource.Columns.Clear();
        DataGridSource.Columns.AddRange(
            Enumerable.Range(0, SensorData.Columns)
                .Select(i => new TextColumn<double[], string>(
                    header: SensorData.Labels?[i] ?? i.ToString(),
                    getter: row => $"{row[i]:0.000}",
                    width: null,
                    options: null
                )));
    }

    private void TreeDataGridRowSelectionModel_PropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (sender is not TreeDataGridRowSelectionModel<double[]> selection ||
            args.PropertyName is not nameof(selection.SelectedIndex) ||
            selection.SelectedIndex.Count < 1)
        {
            return;
        }

        AverageRow = DataProcessor.Instance.AverageOfDatasetRow(SensorData, selection.SelectedIndex[0]);
    }

    private void SensorData_PropertyChanged(object? _, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(SensorData.Labels))
        {
            ApplyDataGridColumns();
        }
    }
}
