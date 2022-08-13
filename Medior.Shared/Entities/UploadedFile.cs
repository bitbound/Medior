using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Medior.Shared.Entities
{
    public class UploadedFile
    {
        public string AccessTokenEdit { get; set; } = string.Empty;
        public string AccessTokenView { get; set; } = string.Empty;
        public string ContentDisposition { get; init; } = string.Empty;
        public string FileName { get; init; } = string.Empty;
        public long FileSize { get; init; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; init; }

        public bool IsEncrypted { get; init; }
        public DateTimeOffset LastAccessed { get; set; }
        public DateTimeOffset UploadedAt { get; init; }
    }
}
