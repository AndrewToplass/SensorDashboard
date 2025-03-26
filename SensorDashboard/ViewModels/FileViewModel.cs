using SensorDashboard.Models;

namespace SensorDashboard.ViewModels;

public class FileViewModel
{
    public required string Title { get; set; }

    public double ThresholdMinimum { get; set; } = 0;
    public double ThresholdMaximum { get; set; } = 0;

    public required SensorData SensorData { get; set; }
}
