using Medior.Shared.Services;
using System;
using System.Windows.Input;

namespace Medior.ViewModels;

public enum KeepAwakeMode
{
    Off,
    Temporary,
    Indefinite
}

internal class KeepAwakeViewModel : ObservableObjectEx
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
            if (value == KeepAwakeExpiration)
            {
                return;
            }
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
            if (value == KeepMonitorAwake)
            {
                return;
            }
            Set(value);
            SetPowerControl();
        }
    }

    public KeepAwakeMode Mode
    {
        get => Get<KeepAwakeMode>();
        set
        {
            if (value == Mode)
            {
                return;
            }
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
                _messenger.SendToast("Keep-awake mode turned off", ToastType.Success);
                break;
            case KeepAwakeMode.Temporary:
                if (!KeepAwakeExpiration.HasValue)
                {
                    _powerControl.DisableKeepAwake();
                    _messenger.SendToast("Expiration time required", ToastType.Warning);
                    return;
                }

                _powerControl.KeepAwake(new DateTimeOffset(KeepAwakeExpiration.Value), KeepMonitorAwake);
                _messenger.SendToast($"Keep-awake set until {KeepAwakeExpiration.Value}", ToastType.Success);
                break;
            case KeepAwakeMode.Indefinite:
                _powerControl.KeepAwake(KeepMonitorAwake);
                _messenger.SendToast($"Keep-awake turned on", ToastType.Success);
                break;
            default:
                break;
        }
    }
}
