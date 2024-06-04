using System.Runtime.Serialization;

namespace Medior.Shared.Entities.Enums;

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
