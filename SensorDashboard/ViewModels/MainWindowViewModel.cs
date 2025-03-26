using System.Collections.ObjectModel;
using Avalonia;
using SensorDashboard.Models;

namespace SensorDashboard.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<FileTabViewModel> OpenTabs { get; set; } = [];
}
