using Medior.Shared.Helpers;

namespace Medior.Web.Client.Services
{
    public interface ILoaderService
    {
        event EventHandler<string>? LoaderShown;
        event EventHandler? LoaderHidden;

        IDisposable Show();
        IDisposable Show(string message);
    }

    public class LoaderService : ILoaderService
    {
        public event EventHandler<string>? LoaderShown;
        public event EventHandler? LoaderHidden;

        public IDisposable Show()
        {
            LoaderShown?.Invoke(this, string.Empty);

            return new CallbackDisposable(() =>
            {
                LoaderHidden?.Invoke(this, EventArgs.Empty);
            });
        }

        public IDisposable Show(string message)
        {
            LoaderShown?.Invoke(this, message);

            return new CallbackDisposable(() =>
            {
                LoaderHidden?.Invoke(this, EventArgs.Empty);
            });
        }
    }
}
