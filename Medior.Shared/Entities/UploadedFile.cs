using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Medior.Shared.Entities;

[DataContract]
public class UploadedFile
{
    [DataMember]
    public string AccessTokenEdit { get; set; } = string.Empty;

    [DataMember]
    public string AccessTokenView { get; set; } = string.Empty;

    [DataMember]
    public string ContentDisposition { get; init; } = string.Empty;

    [DataMember]
    public string FileName { get; init; } = string.Empty;

    [DataMember]
    public long FileSize { get; init; }

    [DataMember]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; init; }


    [DataMember]
    public DateTimeOffset LastAccessed { get; set; }

    [DataMember]
    public DateTimeOffset UploadedAt { get; init; }

}
