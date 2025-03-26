using CommunityToolkit.Mvvm.ComponentModel;

namespace SensorDashboard.Models;

public class SensorData : ObservableObject
{
    public required double[,] Data { get; set; }
    public string[]? Labels { get; set; }
}
