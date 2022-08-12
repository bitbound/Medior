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
        [DataMember]
        public string AccessKey { get; init; } = string.Empty;
    }
}
