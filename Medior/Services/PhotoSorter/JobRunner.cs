﻿using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.ComponentModel;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using Medior.Models.PhotoSorter;

namespace Medior.Services.PhotoSorter;

public interface IJobRunner
{
    Task<JobReport> RunJob(SortJob job, bool dryRun, CancellationToken cancellationToken);

    event ProgressChangedEventHandler ProgressChanged;
    event EventHandler<string> CurrentTaskChanged;
}

public class JobRunner : IJobRunner
{
    private static readonly SemaphoreSlim _runLock = new(1, 1);
    private readonly EnumerationOptions _enumOptions = new()
    {
        AttributesToSkip = FileAttributes.ReparsePoint | FileAttributes.Hidden | FileAttributes.System,
        RecurseSubdirectories = true,
        MatchCasing = MatchCasing.PlatformDefault
    };

    private readonly IFileSystem _fileSystem;
    private readonly ILogger<JobRunner> _logger;
    private readonly IMetadataReader _metaDataReader;
    private readonly IPathTransformer _pathTransformer;

    public JobRunner(IFileSystem fileSystem,
        IMetadataReader metaDataReader,
        IPathTransformer pathTransformer,
        ILogger<JobRunner> logger)
    {
        _fileSystem = fileSystem;
        _metaDataReader = metaDataReader;
        _pathTransformer = pathTransformer;
        _logger = logger;
    }

    public event ProgressChangedEventHandler? ProgressChanged;
    public event EventHandler<string>? CurrentTaskChanged;

    public async Task<JobReport> RunJob(SortJob job, bool dryRun, CancellationToken cancellationToken)
    {
        var jobReport = new JobReport()
        {
            JobName = job.Name,
            Operation = job.Operation,
            DryRun = dryRun
        };

        try
        {
            await _runLock.WaitAsync(cancellationToken);

            _logger.LogInformation("Starting job run: {job}", JsonSerializer.Serialize(job));

            var fileList = new List<string>();

            for (var extIndex = 0; extIndex < job.IncludeExtensions.Length; extIndex++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Job run cancelled.");
                    break;
                }

                var extension = job.IncludeExtensions[extIndex];

                var files = Directory.GetFiles(job.SourceDirectory, $"*.{extension.Replace(".", "")}", _enumOptions)
                    .Where(file => !job.ExcludeExtensions.Any(ext => ext.Equals(Path.GetExtension(file)[1..], StringComparison.OrdinalIgnoreCase)))
                    .ToArray();

                fileList.AddRange(files);
            }


            for (var fileIndex = 0; fileIndex < fileList.Count; fileIndex++)
            {
                var file = fileList[fileIndex];

                CurrentTaskChanged?.Invoke(this, $"File: {Path.GetFileName(file)}");

                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Job run cancelled.");
                    break;
                }

                var result = await PerformFileOperation(job, dryRun, file);
                jobReport.Results.Add(result);

                var progress = (double)fileIndex / fileList.Count * 100;

                ProgressChanged?.Invoke(this, new ProgressChangedEventArgs((int)progress, null));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while running job.");
        }
        finally
        {
            _runLock.Release();
        }

        return jobReport;
    }


    private Task<OperationResult> PerformFileOperation(SortJob job, bool dryRun, string file)
    {
        OperationResult operationResult;
        var exifFound = false;
        var destinationFile = string.Empty;

        try
        {
            var result = _metaDataReader.TryGetExifData(file);

            if (result.IsSuccess && result.Value is not null)
            {
                exifFound = true;
                destinationFile = _pathTransformer.TransformPath(
                    file,
                    job.DestinationFile,
                    result.Value.DateTaken,
                    result.Value.CameraModel);
            }
            else
            {
                exifFound = false;
                var noExifPath = Path.Combine(job.NoExifDirectory, Path.GetFileName(file));
                destinationFile = _pathTransformer.GetUniqueFilePath(noExifPath);
            }

            if (dryRun)
            {
                _logger.LogInformation("Dry run. Skipping file operation.  Source: {file}.  Destination: {destinationFile}.",
                    file,
                    destinationFile);

                operationResult = new OperationResult()
                {
                    FoundExifData = exifFound,
                    PostOperationPath = destinationFile,
                    WasSkipped = true,
                    PreOperationPath = file,
                };

                return Task.FromResult(operationResult);
            }

            if (_fileSystem.FileExists(destinationFile) && job.OverwriteAction == OverwriteAction.Skip)
            {
                _logger.LogWarning("Destination file exists.  Skipping.  Destination file: {destinationFile}", destinationFile);
                operationResult = new OperationResult()
                {
                    FoundExifData = exifFound,
                    WasSkipped = true,
                    PostOperationPath = destinationFile,
                    PreOperationPath = file
                };
                return Task.FromResult(operationResult);
            }

            if (_fileSystem.FileExists(destinationFile) && job.OverwriteAction == OverwriteAction.New)
            {
                _logger.LogWarning("Destination file exists. Creating unique file name.");
                destinationFile = _pathTransformer.GetUniqueFilePath(destinationFile);
            }

            _logger.LogInformation("Starting file operation: {jobOperation}.  Source: {file}.  Destination: {destinationFile}.",
                job.Operation,
                file,
                destinationFile);

            var dirName = Path.GetDirectoryName(destinationFile);
            if (dirName is null)
            {
                throw new DirectoryNotFoundException($"Unable to get directory name for file {destinationFile}.");
            }

            Directory.CreateDirectory(dirName);

            switch (job.Operation)
            {
                case SortOperation.Move:
                    _fileSystem.MoveFile(file, destinationFile, true);
                    break;
                case SortOperation.Copy:
                    _fileSystem.CopyFile(file, destinationFile, true);
                    break;
                default:
                    break;
            }

            operationResult = new OperationResult()
            {
                FoundExifData = exifFound,
                PostOperationPath = destinationFile,
                PreOperationPath = file
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error while running job.");

            operationResult = new OperationResult()
            {
                FoundExifData = exifFound,
                PostOperationPath = destinationFile,
                PreOperationPath = file
            };
        }

        return Task.FromResult(operationResult);
    }
}
