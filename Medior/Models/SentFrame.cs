using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Models
{
    public struct SentFrame
    {
        public SentFrame(long frameSize, DateTimeOffset timestamp)
        {
            Timestamp = timestamp;
            FrameSize = frameSize;
        }

        public DateTimeOffset Timestamp { get; }
        public long FrameSize { get; }
    }
}
