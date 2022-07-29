using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Medior.ViewModels
{
    internal interface IScreenshotViewModel
    {
        ICommand CaptureCommand { get; }
    }
    internal class ScreenshotViewModel : IScreenshotViewModel
    {
        public ScreenshotViewModel()
        {
            CaptureCommand = new AsyncRelayCommand(Capture);
        }

        public ICommand CaptureCommand { get; }

        public async Task Capture()
        {

        }
    }
}
