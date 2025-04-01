using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
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
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        _viewModel.PropertyChanging += ViewModel_PropertyChanging;
        _viewModel.SensorData.PropertyChanged += SensorData_PropertyChanged;
        ApplyGridColumns();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        _viewModel.PropertyChanging -= ViewModel_PropertyChanging;
        _viewModel.SensorData.PropertyChanged -= SensorData_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(_viewModel.SensorData))
        {
            // Attach event listener to SensorData of view model.
            _viewModel.SensorData.PropertyChanged += SensorData_PropertyChanged;
            ApplyGridColumns();
        }
    }

    private void ViewModel_PropertyChanging(object? sender, PropertyChangingEventArgs args)
    {
        if (args.PropertyName is nameof(_viewModel.SensorData))
        {
            // Detach event listener as SensorData object is no longer part of
            // the view model.
            _viewModel.SensorData.PropertyChanged -= SensorData_PropertyChanged;
        }
    }

    private void SensorData_PropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        ApplyGridColumns();
    }

    private void ApplyGridColumns()
    {
        DataGridDisplay.Columns.Clear();
        for (var col = 0; col < _viewModel.SensorData.Columns; col++)
        {
            DataGridDisplay.Columns.Add(new DataGridTextColumn
            {
                Header = _viewModel.SensorData.Labels?[col] ?? col.ToString(),
                Binding = new Binding($"[{col}]")
            });
        }
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
        }
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

    private void ShowHotKeysButton_OnClick(object? sender, RoutedEventArgs e)
    {
    }

    private async void RenameDatasetButton_OnClick(object? sender, RoutedEventArgs e)
    {
        RenameDialogContentViewModel rename = new(_viewModel.SensorData);
        ContentDialog dialog = new()
        {
            Title = "Rename Dataset",
            CloseButtonText = "Cancel",
            PrimaryButtonText = "Rename",
            DefaultButton = ContentDialogButton.Primary,
            Content = rename
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            _viewModel.SensorData.Title = rename.Title;
            _viewModel.SensorData.Labels = rename.Names.Select(c => c.Value).ToArray();
        }
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
        if (!_viewModel.HasUnsavedChanges)
        {
            return true;
        }

        ContentDialog dialog = new()
        {
            Title = "Do you want to save changes?",
            Content = $"There are unsaved changes in “{_viewModel.SensorData.Title}”",
            PrimaryButtonText = "Save",
            SecondaryButtonText = "Don‘t save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await dialog.ShowAsync();

        if (result != ContentDialogResult.Primary)
        {
            // Tab can be close if user chose not to save.
            return result == ContentDialogResult.Secondary;
        }

        var file = await DisplaySaveDialog();
        if (file is null)
        {
            // If user cancelled save from save dialog, tab cannot be closed.
            return false;
        }

        // If user clicked Save, the tab can be closed.
        await _viewModel.SaveFile(file);
        return true;
    }
}
