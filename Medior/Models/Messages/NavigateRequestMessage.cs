using System;

namespace Medior.Models.Messages;

internal class NavigateRequestMessage
{
    public NavigateRequestMessage(Type controlType)
    {
        ControlType = controlType;
    }

    public Type ControlType { get; }
}
