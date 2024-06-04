using MessagePack;
using System.Runtime.Serialization;

namespace Medior.Shared.Dtos;

[DataContract]
public class ClipboardReadyDto
{

    [SerializationConstructor]
    public ClipboardReadyDto(ClipboardSaveDto clipboardSave, string receiptToken)
    {
        ClipboardSave = clipboardSave;
        ReceiptToken = receiptToken;
    }

    [DataMember]
    public ClipboardSaveDto ClipboardSave { get; init; }

    [DataMember]
    public string ReceiptToken { get; init; } = string.Empty;
}
