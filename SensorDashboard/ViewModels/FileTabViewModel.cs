using SensorDashboard.Models;

using CommunityToolkit.Mvvm.ComponentModel;

namespace SensorDashboard.ViewModels;

public partial class FileTabViewModel : ViewModelBase
{
    public string? Title { get; set; }

    [ObservableProperty]
    private double _thresholdMinimum = 20;

    [ObservableProperty]
    private double _thresholdMaximum = 100;

    public required SensorData SensorData { get; set; }
}
