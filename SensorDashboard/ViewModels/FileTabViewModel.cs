using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using SensorDashboard.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SensorDashboard.ViewModels;

public partial class FileTabViewModel : ViewModelBase
{
    public FileTabViewModel()
    {
        PropertyChanged += This_OnPropertyChanged;
        PropertyChanging += This_OnPropertyChanging;
    }

    private void SensorData_OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        HasUnsavedChanges = true;
    }

    private void This_OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(SensorData))
        {
            // Attach event listener to new sensor data property.
            SensorData.PropertyChanged += SensorData_OnPropertyChanged;
        }

        if (args.PropertyName is not nameof(DataGridSelectedIndex))
        {
            return;
        }

        if (DataGridSelectedIndex == -1)
        {
            AverageRow = null;
            return;
        }

        AverageRow = DataProcessor.Instance.AverageOfRow(SensorData,
            DataGridSelectedIndex);
    }

    private void This_OnPropertyChanging(object? sender, PropertyChangingEventArgs args)
    {
        // SensorData may be null on first assignment.
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (args.PropertyName is nameof(SensorData) && SensorData is not null)
        {
            // Detach event listener from now unused sensor data property.
            SensorData.PropertyChanged -= SensorData_OnPropertyChanged;
        }
    }

    private bool _hasUnsavedChanges = false;

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        private set
        {
            SetProperty(ref _hasUnsavedChanges, value);
            FontStyle = value
                ? FontStyle.Italic
                : FontStyle.Normal;
        }
    }

    [ObservableProperty] private FontStyle _fontStyle = FontStyle.Normal;

    [ObservableProperty] private double _thresholdMinimum = 0;

    [ObservableProperty] private double _thresholdMaximum = 100;

    [ObservableProperty] private ObservableCollection<double[]> _dataGridSource = [];

    [ObservableProperty] private int _dataGridSelectedIndex;

    [ObservableProperty] private double _averageTotal;

    [ObservableProperty] private double? _averageRow;

    // Set by required property SensorData.
    private SensorData _sensorData = null!;

    public required SensorData SensorData
    {
        get => _sensorData;
        set
        {
            SetProperty(ref _sensorData, value);
            HasUnsavedChanges = false;

            if (value.Data.Length == 0)
            {
                DataGridSource = [];
                AverageTotal = 0;
                return;
            }

            ObservableCollection<double[]> source = [];
            for (var row = 0; row < value.Rows; row++)
            {
                source.Add(value.GetRow(row).ToArray());
            }

            DataGridSource = source;
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
        HasUnsavedChanges = false;
    }
}
