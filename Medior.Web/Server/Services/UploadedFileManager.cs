using Medior.Shared;
using Medior.Shared.Entities;
using Medior.Shared.Helpers;
using Medior.Shared.Services;
using Medior.Web.Server.Data;
using Medior.Web.Server.Models;

namespace Medior.Web.Server.Services;

public interface IUploadedFileManager
{
    Task<RetrievedFile> Load(Guid fileId);

    Task<UploadedFile> Save(IFormFile uploadedFile);
    Task Delete(Guid id);
    Task<Result<UploadedFile>> GetData(Guid id);
}
public class UploadedFileManager : IUploadedFileManager
{
    private readonly string _appData;
    private readonly AppDb _appDb;
    private readonly IWebHostEnvironment _hostEnv;
    private readonly ISystemTime _systemTime;

    public UploadedFileManager(
        AppDb appDb,
        ISystemTime systemTime,
        IWebHostEnvironment hostEnv)
    {
        _appDb = appDb;
        _hostEnv = hostEnv;
        _systemTime = systemTime;
        _appData = Directory.CreateDirectory(Path.Combine(_hostEnv.ContentRootPath, "App_Data")).FullName;
    }

    public async Task Delete(Guid id)
    {
        var savedFile = await _appDb.UploadedFiles.FindAsync(id);

        if (savedFile is null)
        {
            return;
        }

        _appDb.UploadedFiles.Remove(savedFile);
        await _appDb.SaveChangesAsync();

        try
        {
            var filePath = Path.Combine(_appData, $"{savedFile.Id}{Path.GetExtension(savedFile.FileName)}");
            if (!File.Exists(filePath))
            {
                return;
            }

            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while deleting file: {ex.Message}");
        }

    }

    public async Task<Result<UploadedFile>> GetData(Guid id)
    {
        var uploadedFile = await _appDb.UploadedFiles.FindAsync(id);

        if (uploadedFile is null)
        {
            return Result.Fail<UploadedFile>("File not found.");
        }

        uploadedFile.LastAccessed = _systemTime.Now;
        await _appDb.SaveChangesAsync();

        return Result.Ok(uploadedFile);
    }

    public async Task<RetrievedFile> Load(Guid fileId)
    {
        var uploadedFile = await _appDb.UploadedFiles.FindAsync(fileId);

        if (uploadedFile is null)
        {
            return RetrievedFile.Empty;
        }

        var filePath = Path.Combine(_appData, $"{uploadedFile.Id}{Path.GetExtension(uploadedFile.FileName)}");

        if (!File.Exists(filePath))
        {
            return RetrievedFile.Empty;
        }

        uploadedFile.LastAccessed = _systemTime.Now;
        await _appDb.SaveChangesAsync();

        var fs = new FileStream(filePath, FileMode.Open);

        return new()
        {
            FileStream = fs,
            UploadedFile = uploadedFile,
            Found = true
        };
    }

    public async Task<UploadedFile> Save(IFormFile uploadedFile)
    {
        var uploadEntity = new UploadedFile()
        {
            FileName = uploadedFile.FileName,
            UploadedAt = _systemTime.Now,
            LastAccessed = _systemTime.Now,
            ContentDisposition = uploadedFile.ContentDisposition,
            FileSize = uploadedFile.Length,
            AccessTokenEdit = RandomGenerator.GenerateAccessKey(),
            AccessTokenView = RandomGenerator.GenerateAccessKey(),
        };

        _appDb.UploadedFiles.Add(uploadEntity);
        await _appDb.SaveChangesAsync();

        var filePath = Path.Combine(_appData, $"{uploadEntity.Id}{Path.GetExtension(uploadEntity.FileName)}");
        using var fs = new FileStream(filePath, FileMode.Create);
        await uploadedFile.CopyToAsync(fs);

        return uploadEntity;

    }
}
