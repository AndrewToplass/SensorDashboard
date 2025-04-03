using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace SensorDashboard;

public class SortedObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
{
    private readonly Comparer<T> _comparer = Comparer<T>.Default;

    // Allows CollectionChanged events to be temporarily silenced for non-atomic operations.
    private bool _suppressNotification = false;

    private bool _isMoving = false;

    public SortedObservableCollection()
    {
    }

    public SortedObservableCollection(IEnumerable<T> collection) : base(collection)
    {
        Sort();
    }

    // Listen for property changes to the items in the list, reorder if needed.
    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (sender is not T item)
        {
            return;
        }

        var oldIndex = IndexOf(item);
        if (oldIndex == -1)
        {
            return;
        }

        var newIndex = FindSortedIndex(item);
        MoveItem(oldIndex, newIndex);
    }

    // Find the index for an element in the list, requires the list to be mostly sorted beforehand.
    private int FindSortedIndex(T item)
    {
        var index = 0;
        for (; index < Count; index++)
        {
            if (_comparer.Compare(this[index], item) >= 0)
            {
                break;
            }
        }

        return index;
    }

    protected override void InsertItem(int _, T item)
    {
        if (!_isMoving)
        {
            if (item is not null)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        var index = FindSortedIndex(item);
        base.InsertItem(index, item);
    }

    protected override void RemoveItem(int index)
    {
        if (!_isMoving)
        {
            if (this[index] is not null)
            {
                this[index].PropertyChanged -= Item_PropertyChanged;
            }
        }

        base.RemoveItem(index);
    }

    protected override void SetItem(int index, T item)
    {
        if (this[index] is not null)
        {
            this[index].PropertyChanged -= Item_PropertyChanged;
        }

        if (item is not null)
        {
            item.PropertyChanged += Item_PropertyChanged;
        }

        base.SetItem(index, item);
    }

    protected override void MoveItem(int oldIndex, int _)
    {
        _isMoving = true;
        var newIndex = FindSortedIndex(this[oldIndex]);
        base.MoveItem(oldIndex, newIndex);
        _isMoving = false;
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (!_suppressNotification)
        {
            base.OnCollectionChanged(e);
        }
    }

    public void Sort()
    {
        _suppressNotification = true;
        for (var i = 1; i < Count; i++)
        {
            int j;
            var other = this[i];
            for (j = i; j > 0 && _comparer.Compare(this[j - 1], other) > 0; j--)
            {
                this[j] = this[j - 1];
            }

            this[j] = other;
        }

        _suppressNotification = false;
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public int BinarySearch(T target)
    {
        var min = 0;
        var max = Count - 1;

        while (min <= max)
        {
            var mid = (min + max) / 2;
            switch (_comparer.Compare(target, this[mid]))
            {
                case 0:
                    return mid;

                case < 0:
                    max = mid - 1;
                    break;

                case > 0:
                    min = mid + 1;
                    break;
            }
        }

        return -1;
    }
}
