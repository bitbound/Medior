namespace Medior.Extensions
{
    internal static class IMessengerExtensions
    {
        public static void SendGenericMessage<T>(this IMessenger messenger, T value)
            where T : notnull
        {
            messenger.Send(new GenericMessage<T>(value));
        }

        public static void SendToast(this IMessenger messenger, string message, ToastType type)
        {
            messenger.Send(new ToastMessage(message, type));
        }
    }
}
