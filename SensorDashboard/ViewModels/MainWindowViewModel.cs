using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using SensorDashboard.Models;

namespace SensorDashboard.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        // OpenTabs.CollectionChanged += OpenTabs_CollectionChanged;
        GetOpenTabsAsync = GetOpenTabsAsyncFunc;
    }

    public ObservableCollection<FileTabViewModel> OpenTabs { get; set; } = [];

    [ObservableProperty] private FileTabViewModel? _selectedTab;

    [ObservableProperty] private FileTabViewModel? _tabSearchTarget;

    public Func<string?, CancellationToken, Task<IEnumerable<object>>> GetOpenTabsAsync { get; }

    public Task<IEnumerable<object>> GetOpenTabsAsyncFunc(string? searchText, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return Task.FromResult<IEnumerable<object>>([]);
        }

        return Task.FromResult<IEnumerable<object>>(
            OpenTabs
                .Where(t => t.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase)));
    }

    public void GotoTab(FileTabViewModel? tab)
    {
        if (tab is null)
        {
            return;
        }

        SelectedTab = tab;
    }

    public void CreateNewTab()
    {
        var index = 1;
        string title;
        while (true)
        {
            title = $"Untitled {index}";

            if (OpenTabs.All(t => t.Title != title))
            {
                break;
            }

            index++;
        }

        FileTabViewModel tab = new()
        {
            Title = title,
            SensorData = new()
        };

        OpenTabs.Add(tab);
        SelectedTab = tab;
    }

    public void OpenFile()
    {
        OpenTabs.Add(
            new FileTabViewModel
            {
                Title = $"Sensor Dashboard {OpenTabs.Count + 1}",
                SensorData = new()
                {
                    Data = new double[0, 0]
                }
            });

        SelectedTab = OpenTabs[^1];
    }

    public void SaveFile()
    {
        Console.WriteLine(SelectedTab?.Title ?? "(null)");
    }

    public void CloseFile()
    {
        if (SelectedTab is null)
        {
            return;
        }

        OpenTabs.Remove(SelectedTab);
    }
}
