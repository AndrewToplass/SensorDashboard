using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Platform.Storage;
using SensorDashboard.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;

namespace SensorDashboard.ViewModels;

public partial class FileTabViewModel : ViewModelBase
{
    public FileTabViewModel()
    {
        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is not nameof(DataGridSelectedIndex))
            {
                return;
            }

            if (SensorData is null || DataGridSelectedIndex == -1)
            {
                AverageRow = null;
                return;
            }

            AverageRow = DataProcessor.Instance.AverageOfRow(SensorData,
                DataGridSelectedIndex);
        };
    }

    private bool _hasUnsavedChanges = false;

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        private set => SetProperty(ref _hasUnsavedChanges, value);
    }

    [ObservableProperty] private string _title = "Tab title";

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
        set => SetProperty(ref _sensorData, value);
    }

    public ObservableCollection<DataGridColumn> DataGridColumns { get; set; } = [];

    public override string ToString()
    {
        return Title;
    }

    public void OpenSampleData()
    {
        SensorData = SensorData.FromTest();
        Title = SensorData.Title;
        ObservableCollection<double[]> source = [];

        for (var row = 0; row < SensorData.Data.GetLength(0); row++)
        {
            source.Add(SensorData.GetRow(row).ToArray());
        }

        DataGridColumns.Clear();
        for (var col = 0; col < SensorData.Data.GetLength(1); col++)
        {
            DataGridColumns.Add(new DataGridTextColumn
            {
                Header = SensorData.Labels?[col] ?? col.ToString(),
                Binding = new Binding($"[{col}]"),
            });
        }

        DataGridSource = source;
        AverageTotal = DataProcessor.Instance.Average(SensorData);
    }

    public void OpenFile(IStorageFile file)
    {
    }

    public void SaveFile(IStorageFile file)
    {
    }

    /// <summary>
    /// Prompt user to save changes if the file has unsaved changes before
    /// closing tab.
    /// </summary>
    /// <returns>
    /// Returns True if the tab can be closed, after choosing to save or discard
    /// the file. Returns False if tab cannot be closed, such as choosing the
    /// cancel option.
    /// </returns>
    public async Task<bool> TryClose()
    {
        if (!HasUnsavedChanges)
        {
            return true;
        }

        ContentDialog dialog = new()
        {
            Title = "Do you want to save changes?",
            Content = $"There are unsaved changes in “{Title}”",
            PrimaryButtonText = "Save",
            SecondaryButtonText = "Don't save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // var fileName = DisplaySaveDialog();
        }

        // If user clicked a button other than cancel, the tab can be closed.
        return result != ContentDialogResult.None;
    }
}
