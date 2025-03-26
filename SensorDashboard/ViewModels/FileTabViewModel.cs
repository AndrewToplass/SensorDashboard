using SensorDashboard.Models;

namespace SensorDashboard.ViewModels;

public class FileTabViewModel : ViewModelBase
{
    public string? Title { get; set; }

    public double ThresholdMinimum { get; set; } = 20;
    public double ThresholdMaximum { get; set; } = 100;

    public required SensorData SensorData { get; set; }
}
