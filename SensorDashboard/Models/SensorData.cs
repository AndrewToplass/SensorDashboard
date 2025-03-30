using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SensorDashboard.Models;

public partial class SensorData : ObservableObject
{
    private string _title = null!;

    public required string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    [ObservableProperty] private string[]? _labels;

    [ObservableProperty] private double[,] _data = new double[0, 0];

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

    public static async Task<SensorData> FromFile(Stream stream)
    {
        using BinaryReader reader = new(stream, Encoding.UTF8);

        if (reader.ReadChar() != 'S' ||
            reader.ReadChar() != '4' ||
            reader.ReadChar() != 'U' ||
            reader.ReadChar() != 'D')
        {
            // Make sure the opened file matches the specification.
            throw new IOException("Invalid file format.");
        }

        var rows = reader.ReadInt32();
        var cols = reader.ReadInt32();
        var data = new double[rows, cols];
        var title = reader.ReadString();
        string[]? labels;

        if (reader.ReadBoolean())
        {
            labels = new string[cols];
            for (var i = 0; i < labels.Length; i++)
            {
                labels[i] = reader.ReadString();
            }
        }
        else
        {
            labels = null;
        }

        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < cols; j++)
            {
                data[i, j] = reader.ReadDouble();
            }
        }

        return new SensorData
        {
            Title = title,
            Labels = labels,
            Data = data
        };
    }

    public static async Task<SensorData> FromFile(string fileName)
    {
        await using FileStream fs = new(fileName, FileMode.Open, FileAccess.Read);
        return await FromFile(fs);
    }

    public async Task SaveToFile(Stream stream)
    {
        await using BinaryWriter writer = new(stream, Encoding.UTF8);

        // File identifier.
        writer.Write(['S', '4', 'U', 'D']);

        // Length of data in the file.
        writer.Write(Data.GetLength(0));
        writer.Write(Data.GetLength(1));

        // Title of the dataset.
        writer.Write(Title);

        // Boolean specifies whether dataset contains labelled columns.
        if (Labels is not null)
        {
            writer.Write(true);
            foreach (var label in Labels)
            {
                writer.Write(label);
            }
        }
        else
        {
            writer.Write(false);
        }

        // Sensor data from the 2D array.
        for (var i = 0; i < Data.GetLength(0); i++)
        {
            for (var j = 0; j < Data.GetLength(1); j++)
            {
                writer.Write(Data[i, j]);
            }
        }
    }

    public async Task SaveToFile(string fileName)
    {
        await using FileStream fs = new(fileName, FileMode.Create, FileAccess.Write);
        await SaveToFile(fs);
    }
}
