using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Models.Messages
{
    internal class GenericMessage<T>
        where T : notnull
    {
        public GenericMessage(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }
}
