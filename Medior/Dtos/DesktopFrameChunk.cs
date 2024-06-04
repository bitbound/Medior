using System.Drawing;
using System.Runtime.Serialization;

namespace Medior.Dtos;

[DataContract]
public class DesktopFrameChunk : DtoWrapper
{
    public DesktopFrameChunk()
    {
        DtoType = DtoType.DesktopFrameChunk;
    }

    [DataMember]
    public Rectangle Area { get; init; }

    [DataMember]
    public bool EndOfFrame { get; init; }

    [DataMember]
    public byte[] ImageBytes { get; init; } = [];

}
