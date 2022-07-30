using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Shared.Entities
{
    public class UploadedFile
    {
        public string ContentDisposition { get; init; } = string.Empty;

        public string ContentType { get; init; } = string.Empty;

        public string FileName { get; init; } = string.Empty;

        public long FileSize { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; init; }

        public DateTimeOffset UploadedAt { get; init; }
    }
}
