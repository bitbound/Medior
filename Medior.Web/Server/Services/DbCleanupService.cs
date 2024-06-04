using Medior.Shared.Services;
using Medior.Web.Server.Data;

namespace Medior.Web.Server.Services;

public class DbCleanupService : IHostedService, IDisposable
{
    private readonly SemaphoreSlim _cleanupLock = new(1, 1);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISystemTime _systemTime;
    private System.Timers.Timer? _cleanupTimer;

    public DbCleanupService(IServiceScopeFactory scopeFactory, ISystemTime systemTime)
    {
        _scopeFactory = scopeFactory;
        _systemTime = systemTime;
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        CleanupTimer_Elapsed(this, EventArgs.Empty);
        _cleanupTimer?.Dispose();
        _cleanupTimer = new System.Timers.Timer(TimeSpan.FromDays(1).TotalMilliseconds);
        _cleanupTimer.Elapsed += CleanupTimer_Elapsed;
        _cleanupTimer.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cleanupTimer?.Dispose();
        return Task.CompletedTask;
    }

    private void CleanupClipboardSaves()
    {
        using var scope = _scopeFactory.CreateScope();
        var appDb = scope.ServiceProvider.GetRequiredService<AppDb>();
        var appSettings = scope.ServiceProvider.GetRequiredService<IAppSettings>();

        var expirationDate = _systemTime.Now - TimeSpan.FromDays(appSettings.FileRetentionDays);

        var expiredClips = appDb.ClipboardSaves.Where(x => x.LastAccessed < expirationDate);
        appDb.ClipboardSaves.RemoveRange(expiredClips);
        appDb.SaveChanges();
    }

    private void CleanupFiles()
    {
        using var scope = _scopeFactory.CreateScope();
        var appDb = scope.ServiceProvider.GetRequiredService<AppDb>();
        var hostEnv = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var appSettings = scope.ServiceProvider.GetRequiredService<IAppSettings>();
        var appData = Directory.CreateDirectory(Path.Combine(hostEnv.ContentRootPath, "App_Data")).FullName;

        var expirationDate = _systemTime.Now - TimeSpan.FromDays(appSettings.FileRetentionDays);

        var expiredFiles = appDb.UploadedFiles.Where(x => x.LastAccessed < expirationDate);
        appDb.UploadedFiles.RemoveRange(expiredFiles);
        appDb.SaveChanges();

        foreach (var file in Directory.EnumerateFiles(appData))
        {
            try
            {
                var id = Path.GetFileNameWithoutExtension(file);
                if (!Guid.TryParse(id, out var fileGuid))
                {
                    continue;
                }

                // Delete file if it has expired.
                if (expiredFiles.Any(x => x.Id == fileGuid))
                {
                    File.Delete(file);
                    continue;
                }

                // Delete file if it doesn't exist in the DB anymore.
                if (!appDb.UploadedFiles.Any(x => x.Id == fileGuid))
                {
                    File.Delete(file);
                    continue;
                }
            }

            catch (Exception ex)
            {
                Console.Write($"Error while deleting file: {ex.Message}");
            }
        }
    }

    private void CleanupTimer_Elapsed(object? sender, EventArgs e)
    {
        if (!_cleanupLock.Wait(0))
        {
            return;
        }

        try
        {
            CleanupFiles();
            CleanupClipboardSaves();
        }
        finally
        {
            _cleanupLock.Release();
        }
    }
}
