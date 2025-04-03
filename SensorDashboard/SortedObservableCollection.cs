using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace SensorDashboard;

public class SortedObservableCollection<T> : ObservableCollection<T> where T : IComparable<T>, INotifyPropertyChanged
{
    public SortedObservableCollection() : base()
    {
    }

    public SortedObservableCollection(IEnumerable<T> collection) : base(collection)
    {
    }

    public void Sort()
    {
        throw new NotImplementedException();
    }

    protected override void InsertItem(int _, T item)
    {
        var index = 0;
        for (; index < Count; index++)
        {
            if (this[index].CompareTo(item) > 0)
            {
                break;
            }
        }
        base.InsertItem(index, item);
    }

    public int BinarySearch(T target)
    {
        var min = 0;
        var max = Count - 1;
        int mid;
        int comparison;

        while (min <= max)
        {
            mid = (min + max) / 2;
            comparison = target.CompareTo(this[mid]);

            switch (comparison)
            {
                case 0:
                    return mid;

                case < 0:
                    max = mid - 1;
                    break;

                case > 1:
                    min = mid + 1;
                    break;
            }
        }

        return -1;
    }
}
