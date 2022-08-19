using Medior.Shared.Entities;
using Medior.Shared.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Shared.Dtos
{
    [DataContract]
    public class ClipboardSaveDto
    {
        public ClipboardSaveDto() { }

        public ClipboardSaveDto(ClipboardSave clipboardSave)
        {
            AccessTokenEdit = clipboardSave.AccessTokenEdit;
            AccessTokenView = clipboardSave.AccessTokenView;
            ContentSize = clipboardSave.ContentSize;
            ContentType = clipboardSave.ContentType;
            CreatedAt = clipboardSave.CreatedAt;
            Id = clipboardSave.Id;
            LastAccessed = clipboardSave.LastAccessed;
            FriendlyName = clipboardSave.FriendlyName;
        }

        [DataMember]
        public string FriendlyName { get; init; } = string.Empty;

        [DataMember]
        public string AccessTokenEdit { get; set; } = string.Empty;
        [DataMember]
        public string AccessTokenView { get; set; } = string.Empty;

        [DataMember]
        public long ContentSize { get; init; }

        [DataMember]
        public ClipboardContentType ContentType { get; init; }

        [DataMember]
        public DateTimeOffset CreatedAt { get; init; }

        [DataMember]
        public Guid Id { get; init; }

        [DataMember]
        public bool IsEncrypted { get; init; }

        [DataMember]
        public DateTimeOffset LastAccessed { get; set; }
    }
}
