using SensorDashboard.Models;

using CommunityToolkit.Mvvm.ComponentModel;

namespace SensorDashboard.ViewModels;

public partial class FileTabViewModel : ViewModelBase
{
    public string? Title { get; set; }

    [ObservableProperty]
    private double _thresholdMinimum = 20;

    [ObservableProperty] private string _title = "Tab title";

    [ObservableProperty] private double _thresholdMinimum = 0;

    [ObservableProperty] private double _thresholdMaximum = 100;

    public required SensorData SensorData { get; set; }

    public override string ToString()
    {
        return Title;
    }
}
