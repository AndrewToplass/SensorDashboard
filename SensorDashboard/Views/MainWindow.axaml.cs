using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
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
        ViewModel.Files.Add(
            new()
            {
                Title = $"Sensor Dashboard {ViewModel.Files.Count + 1}",
                SensorData = new()
                {
                    Data = new double[0, 0]
                }
            });
        Tabs.SelectedIndex = ViewModel.Files.Count - 1;
    }

    /// <summary>
    /// Click event for the close tab button.
    /// </summary>
    private void TabClose_OnClick(object? sender, RoutedEventArgs e)
    {
        // Get the view model data that can be removed from open file list.
        if ((sender as Button)?.DataContext is not FileViewModel file)
        {
            return;
        }

        ViewModel.Files.Remove(file);
    }
}
