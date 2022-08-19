using System;

namespace Medior.Extensions
{
    internal static class IMessengerExtensions
    {
        public static void SendGenericMessage<T>(this IMessenger messenger, T value)
            where T : notnull
        {
            messenger.Send(new GenericMessage<T>(value));
        }

        public static void SendParameterlessMessage(this IMessenger messenger, ParameterlessMessageKind kind)
        {
            messenger.Send(new GenericMessage<ParameterlessMessageKind>(kind));
        }

        public static void RegisterParameterless(this IMessenger messenger, 
            object recipient, 
            ParameterlessMessageKind kind, 
            MessageHandler<object, GenericMessage<ParameterlessMessageKind>> handler)
        {
            messenger.Register<GenericMessage<ParameterlessMessageKind>>(recipient, (r, m) =>
            {
                if (m.Value == kind)
                {
                    handler(recipient, m);
                }
            });
        }

        public static void SendToast(this IMessenger messenger, string message, ToastType type)
        {
            messenger.Send(new ToastMessage(message, type));
        }
    }
}
