using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Services.ScreenCapture
{
    public class DxOutput : IDisposable
    {
        public DxOutput(Adapter1 adapter,
            SharpDX.Direct3D11.Device device,
            OutputDuplication outputDuplication,
            Texture2D texture2D,
            DisplayModeRotation rotation)
        {
            Adapter = adapter;
            Device = device;
            OutputDuplication = outputDuplication;
            Texture2D = texture2D;
            Rotation = rotation;
            Bounds = new Rectangle(0, 0, texture2D.Description.Width, texture2D.Description.Height);
        }

        public Adapter1 Adapter { get; }
        public Rectangle Bounds { get; }
        public SharpDX.Direct3D11.Device Device { get; }
        public OutputDuplication OutputDuplication { get; }
        public DisplayModeRotation Rotation { get; }
        public Texture2D Texture2D { get; }

        public void Dispose()
        {
            OutputDuplication.ReleaseFrame();
            TryDispose(OutputDuplication, Texture2D, Adapter, Device);
            GC.SuppressFinalize(this);
        }

        private static void TryDispose(params IDisposable[] disposables)
        {
            foreach (var disposable in disposables)
            {
                try
                {
                    disposable.Dispose();
                }
                catch { }
            }
        }
    }
}
