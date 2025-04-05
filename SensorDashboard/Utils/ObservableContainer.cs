using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SensorDashboard.Utils;

public partial class ObservableContainer<T>(T initial = default!) : ObservableObject
{
    [ObservableProperty] private T _value = initial;

    public override bool Equals(object? obj) => obj is ObservableContainer<T> other && Equals(other);
    protected bool Equals(ObservableContainer<T> other) => EqualityComparer<T>.Default.Equals(Value, other.Value);
    public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(Value!);
}
