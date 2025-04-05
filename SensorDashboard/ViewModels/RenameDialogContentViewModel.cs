using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using SensorDashboard.Models;
using SensorDashboard.Utils;

namespace SensorDashboard.ViewModels;

public partial class RenameDialogContentViewModel(SensorData sensorData) : ViewModelBase
{
    [ObservableProperty] private string _title = sensorData.Title;

    [ObservableProperty] private ObservableCollection<ObservableContainer<string>> _names =
        new((sensorData.Labels ?? new string?[sensorData.Columns])
            .Select(i => new ObservableContainer<string>(i ?? "")));
}
