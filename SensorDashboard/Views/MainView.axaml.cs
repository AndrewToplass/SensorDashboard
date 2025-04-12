using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using SensorDashboard.Models;
using SensorDashboard.ViewModels;

namespace SensorDashboard.Views;

public partial class MainView : UserControl
{
    internal MainViewModel ViewModel { get; private set; } = null!;
    internal bool Closing { get; set; } = false;

    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            ViewModel = vm;
        }

        ViewModel.CreateNewTab();

        base.OnDataContextChanged(e);
    }

    public async Task<bool> RequestCloseTab(FileTabViewModel tab)
    {
        var oldTab = ViewModel.SelectedTab;
        ViewModel.SelectedTab = tab;

        var view = this.FindDescendantOfType<FileTabView>();

        if (!await (view?.TryClose() ?? Task.FromResult(false)))
        {
            return false;
        }

        var index = ViewModel.OpenTabs.IndexOf(tab);
        DataProcessor.Instance.CloseDataset(tab.SensorData);

        var newIndex = index + 1 < ViewModel.OpenTabs.Count
            ? index + 1
            : index - 1;

        ViewModel.SelectedTab = oldTab != tab
            ? oldTab
            : newIndex >= 0
                ? ViewModel.OpenTabs[newIndex]
                : null;

        ViewModel.OpenTabs.Remove(tab);
        if (ViewModel.OpenTabs.Count > 0)
        {
            return true;
        }

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
        else
        {
            ViewModel.CreateNewTab();
        }

        return true;
    }

    private async void Tabs_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is FileTabViewModel tab)
        {
            await RequestCloseTab(tab);
        }
    }
}
