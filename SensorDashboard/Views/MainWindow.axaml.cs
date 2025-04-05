using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using SensorDashboard.ViewModels;
using FluentAvalonia.UI.Windowing;
using SensorDashboard.Models;

namespace SensorDashboard.Views;

public partial class MainWindow : AppWindow
{
    private MainWindowViewModel _viewModel = null!;

    private bool _closing = false;

    public MainWindow()
    {
        InitializeComponent();
        Closing += This_OnClosing;
        DataContextChanged += delegate
        {
            if (DataContext is MainWindowViewModel vm)
            {
                _viewModel = vm;
            }

            _viewModel.CreateNewTab();
            _viewModel.OpenTabs.CollectionChanged += OpenTabs_CollectionChanged;
        };
    }

    private async void This_OnClosing(object? sender, WindowClosingEventArgs args)
    {
        if (_closing)
        {
            args.Cancel = true;
            return;
        }

        if (_viewModel.OpenTabs.Count == 0)
        {
            return;
        }

        _closing = true;
        args.Cancel = true;
        Stack<FileTabViewModel> openTabs = new(_viewModel.OpenTabs);
        while (openTabs.Count > 0)
        {
            if (!await RequestCloseTab(openTabs.Pop()))
            {
                _closing = false;
                break;
            }
        }
    }

    private async Task<bool> RequestCloseTab(FileTabViewModel tab)
    {
        Tabs.SelectedItem = tab;

        var view = this.FindDescendantOfType<FileTabView>();

        if (!await (view?.TryClose() ?? Task.FromResult(false)))
        {
            return false;
        }

        DataProcessor.Instance.CloseDataset(tab.SensorData);
        _viewModel.OpenTabs.Remove(tab);
        return true;
    }

    private void OpenTabs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (_viewModel.OpenTabs.Count > 0)
        {
            return;
        }

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    private async void Tabs_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is FileTabViewModel tab)
        {
            await RequestCloseTab(tab);
        }
    }
}
