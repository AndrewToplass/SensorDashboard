namespace SensorDashboard.ViewModels;

public class FileViewModel(string header, string content)
{
    public string Header { get; } = header;
    public string Content { get; } = content;
}
