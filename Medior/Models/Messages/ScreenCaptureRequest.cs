using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Models.Messages
{
    internal class ScreenCaptureRequest
    {
        public ScreenCaptureRequest(CaptureKind kind)
        {
            Kind = kind;
        }

        public CaptureKind Kind { get; }
    }

    internal enum CaptureKind
    {
        Snip,
        Record
    }
}
