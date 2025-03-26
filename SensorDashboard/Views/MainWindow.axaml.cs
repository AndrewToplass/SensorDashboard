using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using SensorDashboard.ViewModels;

namespace SensorDashboard.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;

    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Click event for the open file button.
    /// </summary>
    private void MenuOpen_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.OpenTabs.Add(
            new FileTabViewModel
            {
                Title = $"Sensor Dashboard {ViewModel.OpenTabs.Count + 1}",
                SensorData = new()
                {
                    Data = new double[0, 0]
                }
            });
        Tabs.SelectedItem = ViewModel.OpenTabs[^1];
    }

    /// <summary>
    /// Click event for the close tab button.
    /// </summary>
    private void Tabs_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is not FileTabViewModel file)
        {
            return;
        }

        ViewModel.OpenTabs.Remove(file);
    }
}
