using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using SensorDashboard.Models;

namespace SensorDashboard.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<FileTabViewModel> OpenTabs { get; set; } = [];

    [ObservableProperty] private FileTabViewModel? _selectedTab;

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
