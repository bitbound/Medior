using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Medior.Reactive
{
    public class ObservableObjectEx : ObservableObject
    {
        private readonly ConcurrentDictionary<string, object?> _backingFields = new();

        protected void Set<T>(T newValue, [CallerMemberName] string propertyName = "")
        {
            _backingFields.AddOrUpdate(propertyName, newValue, (k, v) => newValue);
            OnPropertyChanged(propertyName);
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
