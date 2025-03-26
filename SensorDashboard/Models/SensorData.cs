using CommunityToolkit.Mvvm.ComponentModel;

namespace SensorDashboard.Models;

public class SensorData : ObservableObject
{
    public string? Title { get; set; }

    public string[]? Labels { get; set; }
    public required double[,] Data { get; set; }

    // public static SensorData FromFile(string fileName)
    // {
    // }

    // public void SaveToFile(string fileName)
    // {
    // }
}
