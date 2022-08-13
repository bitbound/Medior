using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Shared.Entities.Enums
{
    [DataContract]
    public enum ClipboardContentType
    {
        [EnumMember]
        Unknown,
        [EnumMember]
        Text,
        [EnumMember]
        Bitmap
    }
}
