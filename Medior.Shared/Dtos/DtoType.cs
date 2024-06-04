using System.Runtime.Serialization;

namespace Medior.Shared.Dtos;

[DataContract]
public enum DtoType
{
    Unknown,
    ClipboardReady,
    UserAccount,
}
