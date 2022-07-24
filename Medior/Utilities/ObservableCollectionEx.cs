using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Utilities
{
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

        /// <summary>
        /// Remove all items from the collection that satisfy the supplied predicate.
        /// </summary>
        /// <param name="predicate">A predicate used to determine which items to remove.</param>
        public void RemoveAll(Predicate<T> predicate)
        {
            // Make a copy so it doesn't throw due to collection
            // changing during enumeration.
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
}
