using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Shared.Entities
{
    [DataContract]
    public class UserAccount
    {
        [DataMember]
        public string Username { get; set; } = string.Empty;

        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; init; }

        [DataMember]
        public string PublicKey { get; set; } = string.Empty;
    }
}
