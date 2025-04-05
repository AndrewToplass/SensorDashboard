using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using SensorDashboard.Models;

namespace SensorDashboard.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<FileTabViewModel> OpenTabs { get; set; } = [];

    [ObservableProperty] private FileTabViewModel? _selectedTab;

    [ObservableProperty] private string? _tabSearchTarget;

    public void SearchForTab(string? title)
    {
        var index = DataProcessor.Instance.Datasets.BinarySearch(title);

        if (index == -1)
        {
            var dialog = new ContentDialog
            {
                Title = "Dataset not found",
                Content = $"Dataset “{title}” was not found in opened datasets.",
                CloseButtonText = "OK"
            };
            _ = dialog.ShowAsync();
            return;
        }

        TabSearchTarget = string.Empty;
        var tab = OpenTabs.FirstOrDefault(t => t.SensorData == DataProcessor.Instance.Datasets[index]);
        SelectedTab = tab;
    }

    public void CreateNewTab()
    {
        var index = 1;
        string title;
        while (true)
        {
            // Find next unique number from untitled open tabs.
            title = $"Untitled {index}";

            if (OpenTabs.All(t => t.SensorData.Title != title))
            {
                break;
            }

            index++;
        }

        FileTabViewModel tab = new()
        {
            SensorData = DataProcessor.Instance.NewDataset(title)
        };

        OpenTabs.Add(tab);
        SelectedTab = tab;
    }
}
