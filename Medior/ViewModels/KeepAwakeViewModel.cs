using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;

namespace Medior.ViewModels
{
    public enum KeepAwakeMode
    {
        Off,
        Temporary,
        Indefinite
    }

    public interface IKeepAwakeViewModel
    {
        DateTime? KeepAwakeExpiration { get; set; }
        bool KeepMonitorAwake { get; set; }
        KeepAwakeMode Mode { get; set; }
        ICommand SetModeCommand { get; }
    }

    internal class KeepAwakeViewModel : ObservableObjectEx, IKeepAwakeViewModel
    {
        private readonly IPowerControl _powerControl;
        private readonly ISystemTime _systemTime;
        private readonly IMessenger _messenger;
        private RelayCommand<KeepAwakeMode>? _setModeCommand;
        public KeepAwakeViewModel(
            IPowerControl powerControl, 
            ISystemTime systemTime,
            IMessenger messenger)
        {
            _powerControl = powerControl;
            _systemTime = systemTime;
            _messenger = messenger;
        }

        public DateTime? KeepAwakeExpiration
        {
            get => Get<DateTime?>();
            set
            {
                if (value < _systemTime.Now)
                {
                    _messenger.Send(new ToastMessage("Time must be in the future", ToastType.Warning));
                    return;
                }
                Set(value);
                SetPowerControl();
            }
        }

        public bool KeepMonitorAwake
        {
            get => Get<bool>();
            set
            {
                Set(value);
                SetPowerControl();
            }
        }

        public KeepAwakeMode Mode
        {
            get => Get<KeepAwakeMode>();
            set
            {
                Set(value);
                SetPowerControl();
            }
        }

        public ICommand SetModeCommand
        {
            get
            {

                return _setModeCommand ??= new RelayCommand<KeepAwakeMode>((parameter) =>
                {
                    Mode = parameter;
                });
            }
        }

        private void SetPowerControl()
        {
            switch (Mode)
            {
                case KeepAwakeMode.Off:
                    _powerControl.DisableKeepAwake();
                    break;
                case KeepAwakeMode.Temporary:
                    if (!KeepAwakeExpiration.HasValue)
                    {
                        return;
                    }

                    _powerControl.KeepAwake(new DateTimeOffset(KeepAwakeExpiration.Value), KeepMonitorAwake);
                    break;
                case KeepAwakeMode.Indefinite:
                    _powerControl.KeepAwake(KeepMonitorAwake);
                    break;
                default:
                    break;
            }
        }
    }
}
