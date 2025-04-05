using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace SensorDashboard;

/// <summary>
/// Like an ObservableCollection&lt;T&gt;, but it is sorted, items are added in
/// order using the default Comparer&lt;T&gt;. Also uses INotifyPropertyChanged
/// on elements to determine changes to sort order due to property changes.
/// </summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
public class SortedCollection<T> : IList<T>, IList, INotifyCollectionChanged, INotifyPropertyChanged
    where T : INotifyPropertyChanged
{
    private readonly List<T> _items;
    private readonly IComparer _comparer;

    /// <summary>
    /// Create a SortedCollection, optionally specifying a Comparer to use.
    /// </summary>
    /// <param name="comparer">Comparer used to sort items by.</param>
    public SortedCollection(IComparer? comparer = null)
    {
        _comparer = comparer ?? Comparer<T>.Default;
        _items = [];
    }

    /// <summary>
    /// Create a SortedCollection&lt;T&gt; from an IEnumerable&lt;T&gt;.
    /// </summary>
    /// <param name="collection">Existing items to add to collection.</param>
    /// <param name="comparer">Comparer used to sort items by.</param>
    public SortedCollection(IEnumerable<T> collection, IComparer? comparer = null)
    {
        _comparer = comparer ?? Comparer<T>.Default;
        var items = collection
            .Select(e =>
            {
                e.PropertyChanged += Item_PropertyChanged;
                return e;
            })
            .ToArray();
        Array.Sort(items, _comparer);
        _items = [..items];
    }

    /// <summary>
    /// Get the number of elements in the collection.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Get a specific item in the collection by index.
    /// </summary>
    /// <param name="index">The index of the element to get.</param>
    public T this[int index]
    {
        get => _items[index];
        set
        {
            _items[index].PropertyChanged -= Item_PropertyChanged;
            _items.RemoveAt(index);

            var (newIndex, _) = FindSortedIndex(value);
            _items.Insert(newIndex, value);

            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    /// <summary>
    /// Add one item to the collection in order from the default
    /// Comparer&lt;T&gt;.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item) => AddItem(item);

    /// <summary>
    /// Remove one item from the collection.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if item removed successfully, false otherwise.</returns>
    public bool Remove(T item)
    {
        item.PropertyChanged -= Item_PropertyChanged;

        var index = BinarySearch(item);
        if (!_items.Remove(item))
        {
            return false;
        }

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        return true;
    }

    /// <summary>
    /// Remove one item from the collection from specified index.
    /// </summary>
    /// <param name="index">Index of item to remove.</param>
    public void RemoveAt(int index)
    {
        var item = _items[index];
        _items.RemoveAt(index);

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
    }

    /// <summary>
    /// Remove all items from the collection.
    /// </summary>
    public void Clear()
    {
        foreach (var item in _items)
        {
            item.PropertyChanged -= Item_PropertyChanged;
        }

        _items.Clear();

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Get the index of a specific item in the collection.
    /// </summary>
    /// <param name="item">Item to get the index of.</param>
    /// <returns>Index of the specified item, or -1 if not found.</returns>
    public int IndexOf(T item) => BinarySearch(item);

    /// <summary>
    /// Perform a linear search on the collection to find specified item.
    /// </summary>
    /// <param name="item">Item to search for.</param>
    /// <returns>Index of the specified item, or -1 if not found.</returns>
    public int LinearSearch(T item)
    {
        for (var i = 0; i < Count; i++)
        {
            if (_comparer.Compare(item, this[i]) == 0)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Perform a binary search on the collection to find specified item.
    /// </summary>
    /// <param name="item">Item to search for.</param>
    /// <returns>Index of the specified item, or -1 if not found.</returns>
    public int BinarySearch(object? item)
    {
        var min = 0;
        var max = Count - 1;

        while (min <= max)
        {
            var mid = (min + max) / 2;
            switch (_comparer.Compare(item, this[mid]))
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

    /// <summary>
    /// Returns whether collection contains specified item.
    /// </summary>
    /// <param name="item">Item to check for in collection.</param>
    /// <returns>True if the item is in the collection, false if not.</returns>
    public bool Contains(T item) => _items.Contains(item);

    /// <summary>
    ///  Occurs when the collection changes.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    /// Occurs when the Index (Item[]) or Count properties change.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }

    protected void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var item = CheckType(sender);
        var oldIndex = LinearSearch(item);
        var (newIndex, matching) = FindSortedIndex(item);

        if (oldIndex >= newIndex && oldIndex < newIndex + matching)
        {
            // If element property change did not change order of list, then
            // nothing needs to be done.
            return;
        }

        _items.RemoveAt(oldIndex);
        _items.Insert(newIndex, item);

        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
    }

    // Helper function to check and enforce types when using non-generic
    // collection interfaces like IList or ICollection.
    private static T CheckType(object? item)
    {
        if (default(T) is not null && item is null)
            throw new ArgumentNullException(nameof(item));

        return (T)item!;
    }

    // IList requires the index returned, while IList<T> doesn't.
    private int AddItem(T item)
    {
        item.PropertyChanged += Item_PropertyChanged;

        var (index, _) = FindSortedIndex(item);
        _items.Insert(index, item);

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));

        return index;
    }

    // Helper that can find an index into an already sorted array for a new
    // element to go, returns index of the first matching element + the number
    // of matching elements.
    private (int first, int matching) FindSortedIndex(T item)
    {
        var index = 0;
        var matches = 0;
        for (; index < Count; index++)
        {
            var comparison = _comparer.Compare(item, this[index]);

            if (comparison == 0)
            {
                matches++;
            }

            if (comparison < 0)
            {
                break;
            }
        }

        return (index - matches, matches);
    }

    // Methods/properties required by interfaces, most are hidden by explicit
    // declarations as they do not need to be used directly.
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
    bool ICollection<T>.IsReadOnly => false;
    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => throw new NotSupportedException();
    void IList<T>.Insert(int _, T item) => Add(item);
    void ICollection<T>.CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
    void ICollection.CopyTo(Array array, int index) => (_items as ICollection).CopyTo(array, index);
    bool IList.IsFixedSize => false;
    bool IList.IsReadOnly => false;
    int IList.Add(object? item) => AddItem(CheckType(item));
    bool IList.Contains(object? item) => (_items as IList).Contains(item);
    int IList.IndexOf(object? item) => IndexOf(CheckType(item));
    void IList.Insert(int _, object? item) => (this as IList).Add(item);
    void IList.Remove(object? item) => Remove(CheckType(item));

    object? IList.this[int index]
    {
        get => this[index];
        set => this[index] = CheckType(value);
    }
}
