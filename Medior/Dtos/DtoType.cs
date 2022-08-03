using System.Runtime.Serialization;

namespace Medior.Dtos
{
    [DataContract]
    public enum DtoType
    {
        Unknown,
        DesktopFrameChunk,
        DisplayList
    }
}
