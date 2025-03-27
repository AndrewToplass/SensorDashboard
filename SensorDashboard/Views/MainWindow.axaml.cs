using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SensorDashboard.ViewModels;
using Avalonia;
using System.Reactive;
using FluentAvalonia.UI.Windowing;
using Avalonia.Styling;

namespace SensorDashboard.Views;

public partial class MainWindow : AppWindow
{
    private MainWindowViewModel _viewModel = null!;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        _viewModel.CreateNewTab();
    }

    private void Tabs_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is FileTabViewModel file)
        {
            _viewModel.OpenTabs.Remove(file);
        }
    }

    // [StructLayout(LayoutKind.Sequential, Pack = 1)]
    // struct MARGINS
    // {
    //     public int cxLeftWidth;
    //     public int cxRightWidth;
    //     public int cxTopHeight;
    //     public int cxBottomHeight;
    // }
    //
    // [DllImport("dwmapi.dll")]
    // static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMargins);
    //
    // void UpdateWin32Border(WindowState state)
    // {
    //     if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    //     {
    //         return;
    //     }
    //
    //     var handle = TryGetPlatformHandle();
    //     if (handle is null)
    //     {
    //         return;
    //     }
    //
    //     MARGINS margins = new();
    //     if (state is not WindowState.Maximized or WindowState.FullScreen)
    //     {
    //         margins.cxLeftWidth = 1;
    //         margins.cxRightWidth = 1;
    //         margins.cxTopHeight = 1;
    //         margins.cxBottomHeight = 1;
    //     }
    //     _ = DwmExtendFrameIntoClientArea(handle.Handle, ref margins);
    // }
}
