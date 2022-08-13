using Medior.Shared.Entities.Enums;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Medior.Shared.Dtos
{
    [DataContract]
    public class ClipboardContentDto
    {
        [JsonConstructor]
        [SerializationConstructor]
        public ClipboardContentDto(string base64Content, ClipboardContentType type) 
        {
            Base64Content = base64Content;
            Type = type;
        }

        public ClipboardContentDto(byte[] imageContent)
        {
            Base64Content = Convert.ToBase64String(imageContent);
            Type = ClipboardContentType.Bitmap;
        }

        public ClipboardContentDto(string textContent)
        {
            Base64Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(textContent));
            Type = ClipboardContentType.Text;
        }

        public ClipboardContentDto(byte[] content, ClipboardContentType contentType) 
        {
            Base64Content = Convert.ToBase64String(content);
            Type = contentType;
        }

        [DataMember]
        public string Base64Content { get; init; } = string.Empty;

        [DataMember]
        public ClipboardContentType Type { get; init; }

        public string GetTextContent()
        {
            if (Type != ClipboardContentType.Text)
            {
                throw new Exception("Type must be text.");
            }

            return Encoding.UTF8.GetString(Convert.FromBase64String(Base64Content));
        }
    }
}
