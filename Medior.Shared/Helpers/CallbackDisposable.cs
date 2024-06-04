namespace Medior.Shared.Helpers;

public class CallbackDisposable : IDisposable
{
    public static CallbackDisposable Empty { get; } = new(() => { });

    private readonly Action _disposeCallback;

    public CallbackDisposable(Action disposeCallback)
    {
        _disposeCallback = disposeCallback;
    }
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        try
        {
            _disposeCallback.Invoke();
        }
        catch { }
    }
}
