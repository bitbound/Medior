using Medior.Shared.Services;
using Microsoft.Extensions.Logging;
using PInvoke;
using System;
using System.Threading;
using Timer = System.Timers.Timer;

namespace Medior.Services
{
    public interface IPowerControl
    {
        void DisableKeepAwake();
        void KeepAwake(bool includeDisplay);
        void KeepAwake(DateTimeOffset until, bool includeDisplay);
    }

    internal class PowerControl : IPowerControl
    {
        private readonly ILogger<PowerControl> _logger;
        private readonly ISystemTime _systemTime;
        private readonly AutoResetEvent _wakeSignal = new(false);
        private Thread? _keepWakeThread;
        private Timer? _timer;
        public PowerControl(ISystemTime systemTime, ILogger<PowerControl> logger)
        {
            _systemTime = systemTime;
            _logger = logger;
        }

        public void DisableKeepAwake()
        {
            _wakeSignal.Set();
        }

        public void KeepAwake(DateTimeOffset until, bool includeDisplay)
        {
            KeepAwake(includeDisplay);

            _timer?.Dispose();

            var due = (until - _systemTime.Now).TotalMilliseconds;

            if (due < 0)
            {
                return;
            }

            _timer = new Timer(due)
            {
                AutoReset = false,
            };

            _timer.Elapsed += (s,e) => _wakeSignal.Set();
            _timer.Start();

        }

        public void KeepAwake(bool includeDisplay)
        {
            _wakeSignal.Set();
            _wakeSignal.Reset();
            _keepWakeThread = new Thread(() =>
            {
                var state = Kernel32.EXECUTION_STATE.ES_CONTINUOUS | Kernel32.EXECUTION_STATE.ES_SYSTEM_REQUIRED;

                if (includeDisplay)
                {
                    state |= Kernel32.EXECUTION_STATE.ES_DISPLAY_REQUIRED;
                }

                var result = Kernel32.SetThreadExecutionState(state);

                if (result == Kernel32.EXECUTION_STATE.None)
                {
                    _logger.LogError("SetThreadExecutionState failed.");
                }

                _wakeSignal.WaitOne();

                Kernel32.SetThreadExecutionState(Kernel32.EXECUTION_STATE.ES_CONTINUOUS);

            });
            _keepWakeThread.Start();
        }
    }
}
