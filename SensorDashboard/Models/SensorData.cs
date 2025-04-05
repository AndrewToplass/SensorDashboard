using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.VisualBasic.FileIO;

namespace SensorDashboard.Models;

public partial class SensorData : ObservableObject
{
    private string _title = null!;

    /// <summary>
    /// Title of the sensor dataset.
    /// </summary>
    public required string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <summary>
    /// Labels for each column of the sensor dataset.
    /// </summary>
    [ObservableProperty] private string[]? _labels;

    /// <summary>
    /// The sensor dataset 2D array containing sensor values.
    /// </summary>
    [ObservableProperty] private double[,] _data = new double[0, 0];

    [ObservableProperty] private bool _hasUnsavedChanges = false;

    public double this[int row, int col]
    {
        get => Data[row, col];
        set => Data[row, col] = value;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(HasUnsavedChanges):
                break;

            default:
                HasUnsavedChanges = true;
                break;
        }

        base.OnPropertyChanged(args);
    }

    /// <summary>
    /// Get the number of rows in the sensor dataset.
    /// </summary>
    public int Rows => Data.GetLength(0);

    /// <summary>
    /// Get the number of columns in the sensor dataset.
    /// </summary>
    public int Columns => Data.GetLength(1);

    /// <summary>
    /// Get an entire row from the sensor dataset.
    /// </summary>
    /// <param name="row">The row index to get.</param>
    /// <returns>An enumerable containing the sensor data from specified row.</returns>
    public IEnumerable<double> GetRow(int row)
    {
        return Enumerable.Range(0, Columns)
            .Select(i => this[row, i]);
    }

    /// <summary>
    /// Generate an example testing dataset.
    /// </summary>
    /// <param name="rows">The number of rows to include in the dataset.</param>
    /// <returns>A new instance containing the test data.</returns>
    public static SensorData FromTest(int rows = 20)
    {
        SensorData sensorData = new()
        {
            Title = "Sensor Data Test",
            Labels = ["First", "Second", "Third", "Fourth", "Fifth"],
            Data = new double[rows, 5]
        };

        for (var i = 0; i < sensorData.Rows; i++)
        {
            for (var j = 0; j < sensorData.Columns; j++)
            {
                sensorData[i, j] = i + j * 1;
            }
        }

        sensorData.HasUnsavedChanges = false;
        return sensorData;
    }

    /// <summary>
    /// Read sensor dataset from stream.
    /// </summary>
    /// <param name="stream">The stream to read data from.</param>
    /// <param name="format">The file format to read the data in.</param>
    /// <param name="title">Optional title to pass to CSV reading, unused for binary reading.</param>
    /// <returns>A new instance containing the parsed data.</returns>
    public static async Task<SensorData> FromStreamAsync(Stream stream, FileFormat format = FileFormat.Binary,
        string? title = null)
    {
        return await (format switch
        {
            FileFormat.Csv => ReadCsvDataAsync(stream, title),
            _ => ReadBinaryDataAsync(stream),
        });
    }

    /// <summary>
    /// Read binary data from stream and get sensor dataset.
    /// </summary>
    /// <param name="stream">The stream to read data from.</param>
    /// <returns>A new instance containing the parsed data.</returns>
    /// <exception cref="IOException">If binary file is invalid or other IO error occured.</exception>
    public static async Task<SensorData> ReadBinaryDataAsync(Stream stream)
    {
        return await Task.Run(() =>
        {
            BinaryReader reader = new(stream, Encoding.UTF8);

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

            var data = new double[rows, cols];
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
                Data = data,
                HasUnsavedChanges = false
            };
        });
    }

    /// <summary>
    /// Read CSV data from stream and get sensor dataset.
    /// </summary>
    /// <param name="stream">The stream to read data from.</param>
    /// <param name="title">Title for the dataset to use.</param>
    /// <returns>A new instance containing the parsed data.</returns>
    public static async Task<SensorData> ReadCsvDataAsync(Stream stream, string? title = null)
    {
        return await Task.Run(() =>
        {
            using StreamReader reader = new(stream);
            using TextFieldParser parser = new(reader);
            parser.TextFieldType = FieldType.Delimited;
            parser.Delimiters = [","];
            parser.HasFieldsEnclosedInQuotes = true;
            parser.TrimWhiteSpace = true;

            var sensorData = new SensorData
            {
                Title = title ?? "Untitled Dataset"
            };

            var header = parser.ReadFields();
            if (header is not null)
            {
                sensorData.Labels = header;
            }

            var columnCount = sensorData.Labels?.Length ?? -1;
            List<double[]> data = [];

            while (!parser.EndOfData)
            {
                var line = parser.ReadFields();
                if (line is null)
                {
                    continue;
                }

                if (columnCount != -1)
                {
                    columnCount = line.Length;
                }

                var row = new double[columnCount];
                for (var i = 0; i < columnCount; i++)
                {
                    var column = i >= columnCount ? null : line[i];
                    if (double.TryParse(column, out var value))
                    {
                        row[i] = value;
                    }
                }

                data.Add(row);
            }

            sensorData.Data = new double[data.Count, columnCount];
            for (var i = 0; i < data.Count; i++)
            {
                for (var j = 0; j < columnCount; j++)
                {
                    sensorData[i, j] = data[i][j];
                }
            }

            sensorData.HasUnsavedChanges = false;
            return sensorData;
        });
    }

    /// <summary>
    /// Save data from current instance to the specified stream, either in
    /// Binary or CSV format.
    /// </summary>
    /// <param name="stream">The stream to write data to.</param>
    /// <param name="format">The file format to write the data in.</param>
    public async Task SaveToStreamAsync(Stream stream, FileFormat format = FileFormat.Binary)
    {
        await (format switch
        {
            FileFormat.Csv => WriteCsvDataAsync(stream),
            _ => WriteBinaryDataAsync(stream),
        });
        HasUnsavedChanges = false;
    }

    /// <summary>
    /// Write sensor dataset to stream in binary format.
    /// </summary>
    /// <param name="stream">The stream to write data to.</param>
    public async Task WriteBinaryDataAsync(Stream stream)
    {
        await using BinaryWriter writer = new(stream, Encoding.UTF8);

        // File identifier.
        writer.Write(['S', '4', 'U', 'D']);

        // Length of data in the file.
        writer.Write(Rows);
        writer.Write(Columns);

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
        for (var i = 0; i < Rows; i++)
        {
            for (var j = 0; j < Columns; j++)
            {
                writer.Write(Data[i, j]);
            }
        }
    }

    /// <summary>
    /// Write sensor dataset to stream in CSV format.
    /// </summary>
    /// <param name="stream">The stream to write data to.</param>
    public async Task WriteCsvDataAsync(Stream stream)
    {
        await using StreamWriter writer = new(stream, Encoding.UTF8);

        // Write labels as header row.
        var headers = Labels?.Select(c => '"' + c.Replace("\"", "\"\"") + '"')
                      ?? Enumerable.Range(0, Columns).Select(i => $"Column {i}");

        await writer.WriteLineAsync(string.Join(", ", headers));

        for (var i = 0; i < Rows; i++)
        {
            // Write each row of data into single line.
            var row = GetRow(i);
            await writer.WriteLineAsync(string.Join(", ", row));
        }

        await writer.WriteLineAsync();
    }
}

public enum FileFormat
{
    Binary,
    Csv
}
