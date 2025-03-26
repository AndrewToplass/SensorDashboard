using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SensorDashboard.ViewModels;

namespace SensorDashboard.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel _viewModel = null!;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        RegisterHotKeys(AppMenu);
    }

    /// <summary>
    /// Menu item gestures do not apply globally, need to find all menu items
    /// with an input gesture and apply a global keybinding.
    /// </summary>
    private void RegisterHotKeys(Control control)
    {
        if (control is MenuItem menuItem)
        {
            if (menuItem.InputGesture is not null && menuItem.Command is not null)
            {
                KeyBindings.Add(new()
                {
                    Gesture = menuItem.InputGesture,
                    Command = menuItem.Command
                });
            }
        }

        foreach (var child in control.GetLogicalChildren().OfType<Control>())
        {
            RegisterHotKeys(child);
        }
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

        _viewModel.OpenTabs.Remove(file);
    }
}
