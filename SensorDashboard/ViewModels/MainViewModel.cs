using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using SensorDashboard.Models;
using SensorDashboard.Views;

namespace SensorDashboard.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ObservableCollection<FileTabViewModel> OpenTabs { get; set; } = [];

    [ObservableProperty] private FileTabViewModel? _selectedTab;

    [ObservableProperty] private string _tabSearchTarget = string.Empty;

    public void SearchForTab()
    {
        var index = DataProcessor.Instance.Datasets.BinarySearch(TabSearchTarget);

        if (index == -1)
        {
            var dialog = new ContentDialog
            {
                Title = "Dataset not found",
                Content = $"Dataset “{TabSearchTarget}” was not found in opened datasets.",
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

    public async void CloseCurrentTab(MainView view)
    {
        if (SelectedTab is null)
        {
            return;
        }

        _ = await view.RequestCloseTab(SelectedTab);
    }

    public void CloseWindow(Window window)
    {
        window.Close();
    }
}
