﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Dtos
{
    [DataContract]
    public class DisplayDto
    {
        [DataMember]
        public int Left { get; init; }

        [DataMember]
        public int Top { get; init; }

        [DataMember]
        public int Right { get; init; }

        [DataMember]
        public int Bottom { get; init; }

        [DataMember]
        public bool IsPrimary { get; init; }

        [DataMember]
        public string DeviceName { get; init; } = string.Empty;

        public int Width => Right - Left;

        public int Height => Bottom - Top;
    }
}
