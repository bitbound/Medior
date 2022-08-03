using System;

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
