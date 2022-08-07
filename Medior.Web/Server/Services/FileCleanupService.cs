using Medior.Web.Server.Data;

namespace Medior.Web.Server.Services
{
    public class FileCleanupService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly SemaphoreSlim _cleanupLock = new(1, 1);

        private System.Timers.Timer? _cleanupTimer;

        public FileCleanupService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
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

        private void CleanupTimer_Elapsed(object? sender, EventArgs e)
        {
            if (!_cleanupLock.Wait(0))
            {
                return;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var appDb = scope.ServiceProvider.GetRequiredService<AppDb>();
                var hostEnv = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                var appSettings = scope.ServiceProvider.GetRequiredService<IAppSettings>();
                var appData = Directory.CreateDirectory(Path.Combine(hostEnv.ContentRootPath, "App_Data")).FullName;

                var expirationDate = DateTimeOffset.Now - TimeSpan.FromDays(appSettings.FileRetentionDays);

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
            finally
            {
                _cleanupLock.Release();
            }
        }
    }
}
