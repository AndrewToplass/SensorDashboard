using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SensorDashboard.ViewModels;

public partial class ErrorDialogContentViewModel(Exception? exception = null) : ViewModelBase
{
    [ObservableProperty] private Exception? _exception = exception;
}
