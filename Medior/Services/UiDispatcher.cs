using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Medior.Services
{
    public interface IUiDispatcher
    {
        void Invoke(Action action);
        DispatcherOperation<Task> InvokeAsync(Func<Task> action);
        DispatcherOperation<Task<T>> InvokeAsync<T>(Func<Task<T>> action);
    }

    internal class UiDispatcher : IUiDispatcher
    {
        public void Invoke(Action action)
        {
            WpfApp.Current.Dispatcher.Invoke(action);
        }

        public DispatcherOperation<Task> InvokeAsync(Func<Task> action)
        {
            return WpfApp.Current.Dispatcher.InvokeAsync(action);
        }

        public DispatcherOperation<Task<T>> InvokeAsync<T>(Func<Task<T>> action)
        {
            return WpfApp.Current.Dispatcher.InvokeAsync(action);
        }
    }
}
