using CommunityToolkit.Mvvm.ComponentModel;

namespace SensorDashboard;

public partial class ObservableContainer<T>(T initial = default!) : ObservableObject
{
    [ObservableProperty] private T _value = initial;

    public static implicit operator ObservableContainer<T>(T container) => new(container);

    public static implicit operator T(ObservableContainer<T> container) => container.Value;
}
