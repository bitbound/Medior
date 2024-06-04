using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Medior.Reactive;

public class ObservableCollectionEx<T> : ObservableCollection<T>
{
    public void AddRange(IEnumerable<T> items)
    {
        if (items?.Any() != true)
        {
            return;
        }

        foreach (var item in items)
        {
            Items.Add(item);
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void NotifyCollectionChanged()
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void RemoveAll(Predicate<T> predicate)
    {
        var copy = Items.ToArray();

        foreach (var item in copy)
        {
            if (predicate(item))
            {
                Items.Remove(item);
            }
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
