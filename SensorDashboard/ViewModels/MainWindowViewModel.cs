﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SensorDashboard.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<FileTabViewModel> OpenTabs { get; set; } = [];

    [ObservableProperty] private FileTabViewModel? _selectedTab;

    [ObservableProperty] private FileTabViewModel? _tabSearchTarget;

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
            SensorData = new()
            {
                Title = title,
                HasUnsavedChanges = false
            }
        };

        OpenTabs.Add(tab);
        SelectedTab = tab;
    }
}
