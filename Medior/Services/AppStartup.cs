using Medior.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Medior.Services;

public interface IAppStartup
{
    Task Initialize(CancellationToken cancellationToken);
}

public class AppStartup : IAppStartup
{
    private readonly ISettings _settings;
    private readonly ITrayService _trayService;
    private readonly IThemeSetter _themeSetter;
    private readonly IRegistryService _registry;
    private readonly IProcessService _processService;
    private readonly IKeyboardHookManager _keyboardHookManager;
    private readonly IEnvironmentHelper _environment;
    private readonly IUiDispatcher _uiDispatcher;
    private readonly IPowerControl _powerControl;
    private readonly ILogger<AppStartup> _logger;
    private readonly IEnumerable<IBackgroundService> _backgroundServices;
    private readonly List<Task> _backgroundTasks = new();
    public AppStartup(
        ISettings settings, 
        ITrayService trayService,
        IThemeSetter themeSetter,
        IKeyboardHookManager keyboardHookManager,
        IRegistryService registry,
        IProcessService processService,
        IEnvironmentHelper environment,
        IUiDispatcher uiDispatcher,
        IPowerControl powerControl,
        ILogger<AppStartup> logger,
        IEnumerable<IBackgroundService> backgroundServices)
    {
        _settings = settings;
        _trayService = trayService;
        _themeSetter = themeSetter;
        _registry = registry;
        _processService = processService;
        _keyboardHookManager = keyboardHookManager;
        _environment = environment;
        _uiDispatcher = uiDispatcher;
        _powerControl = powerControl;
        _logger = logger;
        _backgroundServices = backgroundServices;
    }

    public Task Initialize(CancellationToken cancellationToken)
    {
        StopOtherInstances();
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

        _registry.SetStartAtLogon(_settings.StartAtLogon);

        _uiDispatcher.OnShutdown(ev =>
        {
            _powerControl.DisableKeepAwake();
        });

        return Task.CompletedTask;
    }

    private void StopOtherInstances()
    {
        if (_environment.IsDebug)
        {
            return;
        }

        var procs = _processService
            .GetProcessesByName("Medior")
            .Where(x => x.Id != Environment.ProcessId);

        foreach (var proc in procs)
        {
            try
            {
                proc.Kill(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while stopping other Medior process.");
            }
        }
    }
}
