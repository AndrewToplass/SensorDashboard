using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;
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

    private async void Tabs_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is not FileTabViewModel tab)
        {
            return;
        }

        sender.SelectedItem = tab;

        var view = this.FindDescendantOfType<FileTabView>();

        if (await (view?.TryClose() ?? Task.FromResult(false)))
        {
            _viewModel.OpenTabs.Remove(tab);
        }

        if (_viewModel.OpenTabs.Count > 0)
        {
            return;
        }

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
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

    // private IEnumerable<T> FindAllDescendentsOfType<T>(Visual visual) where T : class
    // {
    //     List<T> result = [];
    //
    //     Stack<Visual> stack = new([visual]);
    //     while (stack.Count > 0)
    //     {
    //         var child = stack.Pop();
    //
    //         if (child is T target)
    //         {
    //             result.Add(target);
    //             continue;
    //         }
    //
    //         foreach (var item in child.GetVisualChildren())
    //         {
    //             stack.Push(item);
    //         }
    //     }
    //
    //     return result;
    // }
}
