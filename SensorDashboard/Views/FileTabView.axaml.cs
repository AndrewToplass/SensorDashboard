using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SensorDashboard.ViewModels;

namespace SensorDashboard.Views;

public partial class FileTabView : UserControl
{
    private FileTabViewModel _viewModel = null!;

    public FileTabView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is not FileTabViewModel vm)
        {
            return;
        }

        _viewModel = vm;


        // Can't use a one-way binding for read-only property.
        foreach (var old in vm.DataGridColumns.OfType<DataGridBoundColumn>())
        {
            // Make sure existing dynamic columns are added back, need to be
            // recreated.
            var column = new DataGridTextColumn
            {
                Header = old.Header,
                Binding = old.Binding,
                Width = old.Width,
                IsReadOnly = old.IsReadOnly,
                IsVisible = old.IsVisible,
                CanUserSort = old.CanUserSort,
                CanUserReorder = old.CanUserReorder,
                CanUserResize = old.CanUserResize
            };
            DataGridDisplay.Columns.Add(column);
        }

        vm.DataGridColumns = DataGridDisplay.Columns;
    }

    public static readonly IReadOnlyList<FilePickerFileType> FileTypes =
    [
        new("Sensing4U Dataset")
        {
            Patterns = ["*.s4u"]
        },
        new("CSV Dataset")
        {
            Patterns = ["*.csv"],
            MimeTypes = ["text/csv"]
        },
    ];

    public async Task<IStorageFile?> DisplayOpenDialog()
    {
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel is null)
        {
            return null;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = "Open Sensor Dataset",
            AllowMultiple = false,
            FileTypeFilter = FileTypes
        });

        return files.Count >= 1
            ? files[0]
            : null;
    }

    public async Task<IStorageFile?> DisplaySaveDialog()
    {
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel is null)
        {
            return null;
        }

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new()
        {
            Title = "Save Sensor Dataset",
            SuggestedFileName = _viewModel.SensorData.Title,
            FileTypeChoices = FileTypes,
            DefaultExtension = ".s4u",
            ShowOverwritePrompt = true
        });

        return file;
    }

    private async void OpenFileButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var file = await DisplayOpenDialog();
        if (file is not null)
        {
            await _viewModel.OpenFile(file);
        }
    }

    private async void SaveFileButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var file = await DisplaySaveDialog();
        if (file is not null)
        {
            await _viewModel.SaveFile(file);
        }
    }

    private void OpenSampleDataButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _viewModel.OpenSampleData();
    }

    private async void ShowHotKeysButton_OnClick(object? sender, RoutedEventArgs e)
    {
    }
}
