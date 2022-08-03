using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Models.Messages
{
    internal class NavigateRequestMessage
    {
        public NavigateRequestMessage(Type controlType)
        {
            ControlType = controlType;
        }

        public Type ControlType { get; }
    }
}
