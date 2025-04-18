using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using Avalonia.Controls;
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
        _viewModel = (DataContext as FileTabViewModel)!;
    }

    public static readonly IReadOnlyList<FilePickerFileType> FileTypes =
    [
        new("Sensing4U Dataset")
        {
            Patterns = ["*.s4u", "*.bin"],
            MimeTypes = ["application/octet-stream"]
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

    private async void ShowErrorMessage(Exception exception, string? action = null)
    {
        var dialog = new ContentDialog
        {
            Title = action is null ? "Error" : $"Error in {action}",
            Content = new ErrorDialogContentViewModel(exception),
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Close
        };
        await dialog.ShowAsync();
    }

    private async void OpenFileButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var previousFile = _viewModel.File;
        try
        {
            var file = await DisplayOpenDialog();
            if (file is not null)
            {
                await _viewModel.OpenFile(file);
            }
        }
        catch (Exception ex)
        {
            _viewModel.File = previousFile;
            ShowErrorMessage(ex, "opening file");
        }
    }

    private async void SaveFileButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var saved = await _viewModel.SaveFile();
            if (saved)
            {
                return;
            }

            var file = await DisplaySaveDialog();
            if (file is not null)
            {
                await _viewModel.SaveFile(file);
            }
        }
        catch (JSException)
        {
            // Browser platform cannot save to existing files.
            SaveFileAsButton_OnClick(sender, e);
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex, "saving file");
        }
    }

    private async void SaveFileAsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var file = await DisplaySaveDialog();
            if (file is not null)
            {
                await _viewModel.SaveFile(file);
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex, "saving file");
        }
    }

    private void OpenSampleDataButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _viewModel.OpenSampleData();
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
        if (!_viewModel.SensorData.HasUnsavedChanges)
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

        try
        {
            var saved = await _viewModel.SaveFile();
            if (saved)
            {
                // File already has path associated, dialog not displayed.
                return true;
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
        catch (Exception ex)
        {
            ShowErrorMessage(ex, "saving file");
            return false;
        }
    }
}
