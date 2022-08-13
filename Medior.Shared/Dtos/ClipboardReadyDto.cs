using Medior.Shared.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Shared.Dtos
{
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
}
