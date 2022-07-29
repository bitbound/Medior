using Medior.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Services
{
    internal interface ICapturePicker
    {
        Rectangle GetCaptureArea();
    }

    internal class CapturePicker : ICapturePicker
    {
        public Rectangle GetCaptureArea()
        {
            var window = new CapturePickerWindow();
            window.ShowDialog();
            return window.SelectedArea;
        }
    }
}
