using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Dtos
{
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
        public byte[] ImageBytes { get; init; } = Array.Empty<byte>();

    }
}
