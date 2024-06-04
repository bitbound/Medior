using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Medior.Services;

public interface IUiDispatcher
{
    void Invoke(Action action);
    DispatcherOperation<Task> InvokeAsync(Func<Task> action);
    DispatcherOperation<Task<T>> InvokeAsync<T>(Func<Task<T>> action);
    void OnShutdown(Action<ExitEventArgs> action);
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

    public void OnShutdown(Action<ExitEventArgs> action)
    {
        WpfApp.Current.Exit += (sender, ev) => 
        {
            action.Invoke(ev);
        };
    }
}
