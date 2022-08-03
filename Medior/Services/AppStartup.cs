using Medior.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface IAppStartup
    {
        Task Initialize(CancellationToken cancellationToken);
    }

    public class AppStartup : IAppStartup
    {
        private readonly ISettings _settings;
        private readonly ITrayService _trayService;
        private readonly IThemeSetter _themeSetter;
        private readonly IKeyboardHookManager _keyboardHookManager;
        private readonly IEnumerable<IBackgroundService> _backgroundServices;
        private readonly List<Task> _backgroundTasks = new();
        public AppStartup(
            ISettings settings, 
            ITrayService trayService,
            IThemeSetter themeSetter,
            IKeyboardHookManager keyboardHookManager,
            IEnumerable<IBackgroundService> backgroundServices)
        {
            _settings = settings;
            _trayService = trayService;
            _themeSetter = themeSetter;
            _keyboardHookManager = keyboardHookManager;
            _backgroundServices = backgroundServices;
        }

        public Task Initialize(CancellationToken cancellationToken)
        {
            foreach (var backgroundService in _backgroundServices)
            {
                _backgroundTasks.Add(backgroundService.Start(cancellationToken));
            }

            _trayService.Initialize();
            _themeSetter.SetTheme(_settings.Theme);

            if (_settings.HandlePrintScreen)
            {
                _keyboardHookManager.SetPrintScreenHook();
            }

            return Task.CompletedTask;
        }
    }
}
