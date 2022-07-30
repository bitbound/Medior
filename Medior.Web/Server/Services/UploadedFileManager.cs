using Medior.Shared.Entities;
using Medior.Web.Server.Data;
using Medior.Web.Server.Models;

namespace Medior.Web.Server.Services
{
    public interface IUploadedFileManager
    {
        Task<RetrievedFile> Load(Guid fileId);

        Task<UploadedFile> Save(IFormFile uploadedFile);
        Task Delete(string id);
    }
    public class UploadedFileManager : IUploadedFileManager
    {
        private readonly string _appData;
        private readonly AppDb _appDb;
        private readonly IWebHostEnvironment _hostEnv;
        private readonly ILogger<UploadedFileManager> _logger;

        public UploadedFileManager(
            AppDb appDb,
            IWebHostEnvironment hostEnv,
            ILogger<UploadedFileManager> logger)
        {
            _appDb = appDb;
            _hostEnv = hostEnv;
            _logger = logger;
            _appData = Directory.CreateDirectory(Path.Combine(_hostEnv.ContentRootPath, "App_Data")).FullName;
        }

        public async Task Delete(string id)
        {
            if (!Guid.TryParse(id, out var idResult))
            {
                return;
            }

            var savedFile = await _appDb.SavedFiles.FindAsync(idResult);

            if (savedFile is not null)
            {
                _appDb.SavedFiles.Remove(savedFile);
                await _appDb.SaveChangesAsync();
            }

            try
            {
                if (Directory.Exists(_appData))
                {
                    var fsFile = Directory.EnumerateFiles(_appData).FirstOrDefault(x => x.Contains(id));
                    if (fsFile is not null)
                    {
                        File.Delete(fsFile);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while deleting file: {ex.Message}");
            }

        }

        public async Task<RetrievedFile> Load(Guid fileId)
        {
            var savedFile = await _appDb.SavedFiles.FindAsync(fileId);

            if (savedFile is null)
            {
                return RetrievedFile.Empty;
            }

            var filePath = Path.Combine(_appData, $"{savedFile.Id}{Path.GetExtension(savedFile.FileName)}");

            if (!File.Exists(filePath))
            {
                return RetrievedFile.Empty;
            }

            var fs = new FileStream(filePath, FileMode.Open);

            return new()
            {
                FileStream = fs,
                UploadedFile = savedFile,
                Found = true
            };
        }

        public async Task<UploadedFile> Save(IFormFile uploadedFile)
        {
            var savedFile = new UploadedFile()
            {
                FileName = uploadedFile.FileName,
                UploadedAt = DateTimeOffset.Now,
                ContentDisposition = uploadedFile.ContentDisposition,
                ContentType = uploadedFile.ContentType,
                FileSize = uploadedFile.Length
            };

            _appDb.SavedFiles.Add(savedFile);
            await _appDb.SaveChangesAsync();

            var filePath = Path.Combine(_appData, $"{savedFile.Id}{Path.GetExtension(savedFile.FileName)}");
            using var fs = new FileStream(filePath, FileMode.Create);
            await uploadedFile.CopyToAsync(fs);

            return savedFile;

        }
    }
}
