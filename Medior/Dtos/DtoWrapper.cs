using System;
using System.Runtime.Serialization;

namespace Medior.Dtos;

[DataContract]
public class DtoWrapper
{
    [DataMember]
    public byte[] DtoChunk { get; init; } = [];

    [DataMember]
    public DtoType DtoType { get; init; }

    [DataMember]
    public bool IsFirstChunk { get; init; }

    [DataMember]
    public bool IsLastChunk { get; init; }

    [DataMember]
    public Guid RequestId { get; init; }

    [DataMember]
    public Guid ResponseId { get; init; }

    [DataMember]
    public int SequenceId { get; init; }
}
