using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SensorDashboard.Models;

public class SensorData : ObservableObject
{
    public required string Title { get; set; }

    public string[]? Labels { get; set; }
    public double[,] Data { get; set; } = new double[0, 0];

    public static SensorData FromTest()
    {
        SensorData sensorData = new()
        {
            Title = "Sensor Data Test",
            Labels = ["First", "Second", "Third", "Fourth", "Fifth"],
            Data = new double[100, 5]
        };

        for (var i = 0; i < sensorData.Data.GetLength(0); i++)
        {
            for (var j = 0; j < sensorData.Data.GetLength(1); j++)
            {
                sensorData[i, j] = i + j * 1;
            }
        }

        return sensorData;
    }

    public double this[int row, int col]
    {
        get => Data[row, col];
        set => Data[row, col] = value;
    }

    public IEnumerable<double> GetRow(int row)
    {
        return Enumerable.Range(0, Data.GetLength(1))
            .Select(i => Data[row, i]);
    }

    public IEnumerable<double> GetColumn(int col)
    {
        return Enumerable.Range(0, Data.GetLength(0))
            .Select(i => Data[i, col]);
    }

    // public static SensorData FromFile(string fileName)
    // {
    // }

    // public void SaveToFile(string fileName)
    // {
    // }
}
