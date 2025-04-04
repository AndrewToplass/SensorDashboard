using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using SensorDashboard.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;

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

    [ObservableProperty] private double _thresholdMinimum = 5;

    [ObservableProperty] private double _thresholdMaximum = 15;

    [ObservableProperty] private FlatTreeDataGridSource<double[]> _dataGridSource = new([]);

    [ObservableProperty] private double _averageTotal;

    [ObservableProperty] private double? _averageRow;

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

            if (SensorData.Data.Length == 0)
            {
                DataGridSource.Items = [];
                AverageTotal = 0;
                return;
            }

            DataGridSource.Items = Enumerable.Range(0, SensorData.Rows)
                .Select(i => SensorData.GetRow(i).ToArray()).ToArray();
            AverageTotal = DataProcessor.Instance.Average(SensorData);
        }
    }

    public override string ToString()
    {
        return SensorData.Title;
    }

    public void OpenSampleData()
    {
        SensorData = SensorData.FromTest();
    }

    public async Task OpenFile(IStorageFile file)
    {
        await using var stream = await file.OpenReadAsync();
        var format = file.Name.EndsWith(".csv") ? FileFormat.Csv : FileFormat.Binary;
        SensorData = await SensorData.FromStream(stream, format, file.Name);
    }

    public async Task SaveFile(IStorageFile file)
    {
        var format = file.Name.EndsWith(".csv") ? FileFormat.Csv : FileFormat.Binary;
        await using var stream = await file.OpenWriteAsync();
        await SensorData.SaveToStream(stream, format);
    }

    private void ApplyDataGridColumns()
    {
        DataGridSource.Columns.Clear();
        DataGridSource.Columns.AddRange(
            Enumerable.Range(0, SensorData.Columns)
                .Select(i => new TextColumn<double[], double>(
                    header: SensorData.Labels?[i] ?? i.ToString(),
                    getter: row => row[i],
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

        AverageRow = DataProcessor.Instance.AverageOfRow(SensorData, selection.SelectedIndex[0]);
    }

    private void SensorData_PropertyChanged(object? _, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(SensorData.Labels))
        {
            ApplyDataGridColumns();
        }
    }
}
