using System.Collections.Generic;
using Avalonia.Controls;
using SensorDashboard.ViewModels;
using FluentAvalonia.UI.Windowing;

namespace SensorDashboard.Views;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        InitializeComponent();
        Closing += This_OnClosing;
    }

    private async void This_OnClosing(object? sender, WindowClosingEventArgs args)
    {
        if (MainView.Closing)
        {
            args.Cancel = true;
            return;
        }

        if (MainView.ViewModel.OpenTabs.Count == 0)
        {
            return;
        }

        MainView.Closing = true;
        args.Cancel = true;
        Stack<FileTabViewModel> openTabs = new(MainView.ViewModel.OpenTabs);
        while (openTabs.Count > 0)
        {
            if (!await MainView.RequestCloseTab(openTabs.Pop()))
            {
                MainView.Closing = false;
                break;
            }
        }
    }
}
