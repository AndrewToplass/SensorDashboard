using System.Collections.ObjectModel;
using Avalonia;
using SensorDashboard.Models;

namespace SensorDashboard.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public SensorData? SensorData { get; set; }

    public ObservableCollection<FileViewModel> Files { get; set; } = [];
}
