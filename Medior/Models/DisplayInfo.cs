using System;
using System.Drawing;
using System.Numerics;

namespace Medior.Models
{
    public class DisplayInfo
    {
        public bool IsPrimary { get; set; }
        public Vector2 ScreenSize { get; set; }
        public Rectangle MonitorArea { get; set; }
        public Rectangle WorkArea { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public IntPtr Hmon { get; set; }
    }
}
