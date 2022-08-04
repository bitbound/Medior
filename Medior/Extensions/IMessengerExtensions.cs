using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
