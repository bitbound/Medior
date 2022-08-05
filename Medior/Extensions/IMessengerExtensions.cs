using CommunityToolkit.Mvvm.Messaging;

namespace Medior.Extensions
{
    internal static class IMessengerExtensions
    {
        public static void SendGenericMessage<T>(this IMessenger messenger, T value)
            where T : notnull
        {
            messenger.Send(new GenericMessage<T>(value));
        }
    }
}
