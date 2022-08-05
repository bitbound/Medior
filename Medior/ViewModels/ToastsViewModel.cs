using System.Threading.Tasks;

namespace Medior.ViewModels
{
    public interface IToastsViewModel
    {
        ObservableCollectionEx<ToastMessage> Toasts { get; }
    }
    internal class ToastsViewModel : IToastsViewModel
    {
        private readonly IMessenger _messenger;
        private readonly IUiDispatcher _uiDispatcher;

        public ToastsViewModel(IMessenger messenger, IUiDispatcher uiDispatcher)
        {
            _messenger = messenger;
            _uiDispatcher = uiDispatcher;
            _messenger.Register<ToastMessage>(this, HandleToastMessage);
        }

        public ObservableCollectionEx<ToastMessage> Toasts { get; } = new();

        private void HandleToastMessage(object recipient, ToastMessage message)
        {
            _uiDispatcher.Invoke(() =>
            {
                Toasts.Add(message);

            });

            _ = Task.Run(async () =>
            {
                await Task.Delay(4_000);
                _uiDispatcher.Invoke(() =>
                {
                    Toasts.Remove(message);
                });
            });
            
        }
    }
}
