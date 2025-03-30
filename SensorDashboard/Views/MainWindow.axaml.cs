using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using SensorDashboard.ViewModels;
using FluentAvalonia.UI.Windowing;

namespace SensorDashboard.Views;

public partial class MainWindow : AppWindow
{
    private MainWindowViewModel _viewModel = null!;

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += delegate
        {
            if (DataContext is MainWindowViewModel vm)
            {
                _viewModel = vm;
            }
            _viewModel.CreateNewTab();
        };
    }

    private void Tabs_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is not FileTabViewModel file)
        {
            return;
        }

        file.TryClose()
            .ContinueWith(async task =>
            {
                if (await task)
                {
                    _viewModel.OpenTabs.Remove(file);
                }
            });
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
