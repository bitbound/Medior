using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace Medior.Services
{
    public interface IFileSystem
    {
        string AddSharedStorageFile(StorageFile storageFile);

        Task AppendAllLinesAsync(string path, IEnumerable<string> lines);
        void CleanupTempFiles();

        void CopyFile(string sourceFile, string destinationFile, bool overwrite);
        DirectoryInfo CreateDirectory(string directoryPath);

        Stream CreateFile(string filePath);
        FileStream CreateFileStream(string filePath, FileMode mode);

        void Encrypt(string filePath);

        bool FileExists(string path);
        Task<StorageFile> GetFileFromPathAsync(string filePath);

        void MoveFile(string sourceFile, string destinationFile, bool overwrite);
        string ReadAllText(string filePath);
        Task<string> ReadAllTextAsync(string path);
        void WriteAllText(string filePath, string contents);
        Task WriteAllTextAsync(string path, string content);
    }

    public class FileSystem : IFileSystem
    {
        public string AddSharedStorageFile(StorageFile storageFile)
        {
            return SharedStorageAccessManager.AddFile(storageFile);
        }

        public Task AppendAllLinesAsync(string path, IEnumerable<string> lines)
        {
            return File.AppendAllLinesAsync(path, lines);
        }

        public void CleanupTempFiles()
        {
            var expiredRecordings = Directory
                    .GetFiles(AppConstants.RecordingsDirectory)
                    .Select(x => new FileInfo(x))
                    .OrderByDescending(x => x.CreationTime)
                    .Skip(20);

            var expiredImages = Directory
                    .GetFiles(AppConstants.ImagesDirectory)
                    .Select(x => new FileInfo(x))
                    .OrderByDescending(x => x.CreationTime)
                    .Skip(20);

            foreach (var file in expiredRecordings.Concat(expiredImages))
            {
                try
                {
                    file.Delete();
                }
                catch { }
            }
        }

        public void CopyFile(string sourceFile, string destinationFile, bool overwrite)
        {
            File.Copy(sourceFile, destinationFile, overwrite);
        }

        public DirectoryInfo CreateDirectory(string directoryPath)
        {
            return Directory.CreateDirectory(directoryPath);
        }

        public Stream CreateFile(string filePath)
        {
            return File.Create(filePath);
        }

        public FileStream CreateFileStream(string filePath, FileMode mode)
        {
            return new FileStream(filePath, mode);
        }

        public void Encrypt(string filePath)
        {
            File.Encrypt(filePath);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public async Task<StorageFile> GetFileFromPathAsync(string filePath)
        {
            return await StorageFile.GetFileFromPathAsync(filePath);
        }

        public void MoveFile(string sourceFile, string destinationFile, bool overwrite)
        {
            File.Move(sourceFile, destinationFile, overwrite);
        }

        public string ReadAllText(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        public Task<string> ReadAllTextAsync(string path)
        {
            return File.ReadAllTextAsync(path);
        }

        public void WriteAllText(string filePath, string contents)
        {
            File.WriteAllText(filePath, contents);
        }

        public Task WriteAllTextAsync(string path, string content)
        {
            return File.WriteAllTextAsync(path, content);
        }
    }
}
