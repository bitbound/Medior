using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Reactive
{
    internal class ObservableObject : INotifyPropertyChanged
    {
        private readonly ConcurrentDictionary<string, object?> _backingFields = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = "")
        {
            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void Set<T>(T newValue, [CallerMemberName] string propertyName = "")
        {
            _backingFields.AddOrUpdate(propertyName, newValue, (k, v) => newValue);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected T? Get<T>([CallerMemberName] string propertyName = "", T? defaultValue = default)
        {
            if (_backingFields.TryGetValue(propertyName, out var value) &&
                value is T typedValue)
            {
                return typedValue;
            }

            return defaultValue;
        }
    }
}
