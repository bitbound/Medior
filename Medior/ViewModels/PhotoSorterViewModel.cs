using CommunityToolkit.Diagnostics;
using MahApps.Metro.Controls.Dialogs;
using Medior.Models.PhotoSorter;
using Medior.Services.PhotoSorter;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Medior.ViewModels
{
    public class PhotoSorterViewModel : ObservableObjectEx
    {
        private readonly IDialogService _dialogService;
        private readonly IUiDispatcher _dispatcher;
        private readonly IJobRunner _jobRunner;
        private readonly ILogger<PhotoSorterViewModel> _logger;
        private readonly IMessenger _messenger;
        private readonly IPathTransformer _pathTransformer;
        private readonly IProcessService _processService;
        private readonly IReportWriter _reportWriter;
        private readonly ISettings _settings;
        private RelayCommand? _cancelJobCommand;
        private AsyncRelayCommand? _createNewJobCommand;
        private AsyncRelayCommand? _deleteJobCommand;
        private CancellationToken _jobCancelToken;
        private CancellationTokenSource? _jobCancelTokenSource;
        private RelayCommand? _openReportsDirectoryCommand;
        private AsyncRelayCommand? _renameJobCommand;
        private RelayCommand? _saveJobCommand;
        private AsyncRelayCommand? _showDestinationTransformCommand;
        private AsyncRelayCommand? _startJobCommand;
        public PhotoSorterViewModel(
            IPathTransformer pathTransformer,
            IJobRunner jobRunner,
            IReportWriter reportWriter,
            IUiDispatcher dispatcher,
            IDialogService dialogService,
            ISettings settings,
            IMessenger messenger,
            IProcessService processService,
            ILogger<PhotoSorterViewModel> logger)
        {
            _pathTransformer = pathTransformer;
            _jobRunner = jobRunner;
            _reportWriter = reportWriter;
            _dispatcher = dispatcher;
            _dialogService = dialogService;
            _settings = settings;
            _messenger = messenger;
            _processService = processService;
            _logger = logger;

            _jobRunner.ProgressChanged += JobRunner_ProgressChanged;
            _jobRunner.CurrentTaskChanged += JobRunner_CurrentTaskChanged;

            LoadSortJobs();
        }

        public ICommand CancelJobCommand
        {
            get
            {
                return _cancelJobCommand ??= new RelayCommand(CancelJob, () => IsJobRunning);
            }
        }

        public ICommand CreateNewJobCommand
        {
            get
            {
                return _createNewJobCommand ??= new AsyncRelayCommand(CreateNewJob);
            }
        }

        public string CurrentJobRunnerTask
        {
            get => Get<string>() ?? "";
            set => Set(value);
        }

        public ICommand DeleteJobCommand
        {
            get
            {
                return _deleteJobCommand ??= new AsyncRelayCommand(DeleteSortJob, () => SelectedJob is not null);
            }
        }

        public string ExcludedExtenions
        {
            get
            {
                if (SelectedJob?.ExcludeExtensions?.Any() != true)
                {
                    return string.Empty;
                }

                return string.Join(",", SelectedJob.ExcludeExtensions);
            }
            set
            {
                if (SelectedJob is null)
                {
                    return;
                }

                SelectedJob.ExcludeExtensions = value
                    .Split(",")
                    .Select(x => x.Trim())
                    .ToArray();
            }
        }

        public string IncludedExtensions
        {
            get
            {
                if (SelectedJob?.IncludeExtensions?.Any() != true)
                {
                    return string.Empty;
                }

                return string.Join(",", SelectedJob.IncludeExtensions);
            }
            set
            {
                if (SelectedJob is null)
                {
                    return;
                }

                SelectedJob.IncludeExtensions = value
                    .Split(",")
                    .Select(x => x.Trim())
                    .ToArray();
            }
        }

        public bool IsDryRun
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool IsJobRunning
        {
            get => Get<bool>();
            set => Set(value);
        }

        public int JobRunnerProgress
        {
            get => Get<int>();
            set
            {
                Set(value);
                OnPropertyChanged(nameof(JobRunnerProgressPercent));
            }
        }

        public string JobRunnerProgressPercent
        {
            get => $"{JobRunnerProgress}%";
        }

        public ICommand OpenReportsDirectoryCommand
        {
            get
            {
                return _openReportsDirectoryCommand ??= new RelayCommand(OpenReportsDirectory);
            }
        }

        public OverwriteAction[] OverwriteActions { get; } = Enum.GetValues<OverwriteAction>();

        public ICommand RenameJobCommand
        {
            get
            {
                return _renameJobCommand ??= new AsyncRelayCommand(RenameSortJob, () => SelectedJob is not null);
            }
        }

        public ICommand SaveJobCommand
        {
            get
            {
                return _saveJobCommand ??= new RelayCommand(SaveJob, () => SelectedJob is not null);
            }
        }

        public SortJob? SelectedJob
        {
            get => Get<SortJob?>();
            set
            {
                Set(value);
                OnPropertyChanged(nameof(IncludedExtensions));
                OnPropertyChanged(nameof(ExcludedExtenions));
            }
        }

        public ICommand ShowDestinationTransformCommand
        {
            get
            {
                return _showDestinationTransformCommand ??=
                    new AsyncRelayCommand(ShowDestinationTransform, () =>
                        !string.IsNullOrWhiteSpace(SelectedJob?.DestinationFile));
            }
        }

        public ObservableCollectionEx<SortJob> SortJobs { get; } = new();

        public SortOperation[] SortOperations { get; } = Enum.GetValues<SortOperation>();

        public ICommand StartJobCommand
        {
            get
            {
                return _startJobCommand ??= new AsyncRelayCommand(StartJob, () =>
                    SelectedJob is not null &&
                    !IsJobRunning);
            }
        }

        public async Task DeleteSortJob()
        {
            var result = await _dialogService.ShowMessageAsync(
                "Confirm Delete",
                "Are you sure you want to delete this sort job?",
                MessageDialogStyle.AffirmativeAndNegative);

            if (result != MessageDialogResult.Affirmative)
            {
                return;
            }

            if (SelectedJob is not null)
            {
                SortJobs.Remove(SelectedJob);
                Save();
                LoadSortJobs();
                SelectedJob = SortJobs.FirstOrDefault();
            }
        }

        public string GetDestinationTransform()
        {
            if (string.IsNullOrWhiteSpace(SelectedJob?.DestinationFile))
            {
                return string.Empty;
            }

            return _pathTransformer.TransformPath(
                "Example.ext",
                SelectedJob.DestinationFile,
                DateTime.Now,
                "Example Camera");
        }

        public OverwriteAction[] GetOverwriteActions()
        {
            return Enum.GetValues<OverwriteAction>();
        }

        public SortOperation[] GetSortOperations()
        {
            return Enum.GetValues<SortOperation>();
        }

        public void LoadSortJobs()
        {
            SortJobs.Clear();

            foreach (var job in _settings.SortJobs.OrderBy(x => x.Name))
            {
                SortJobs.Add(job);
            }
            OnPropertyChanged(nameof(IncludedExtensions));
            OnPropertyChanged(nameof(ExcludedExtenions));
        }

        public void NotifyCommandsCanExecuteChanged()
        {
            _dispatcher.Invoke(() =>
            {
                _cancelJobCommand?.NotifyCanExecuteChanged();
                _createNewJobCommand?.NotifyCanExecuteChanged();
                _deleteJobCommand?.NotifyCanExecuteChanged();
                _renameJobCommand?.NotifyCanExecuteChanged();
                _saveJobCommand?.NotifyCanExecuteChanged();
                _showDestinationTransformCommand?.NotifyCanExecuteChanged();
                _startJobCommand?.NotifyCanExecuteChanged();
            });
        }

        public async Task RenameSortJob()
        {
            var result = await _dialogService.ShowInputAsync("Rename Sort Job", "Enter a new name for the sort job");

            if (string.IsNullOrWhiteSpace(result))
            {
                return;
            }

            var modifiedJob = SortJobs.FirstOrDefault(x => x.Id == SelectedJob?.Id);
            if (modifiedJob is null)
            {
                return;
            }

            modifiedJob.Name = result;
            Save();
            LoadSortJobs();
            SelectedJob = SortJobs.FirstOrDefault(x => x.Id == modifiedJob.Id);
        }

        public void SaveJob()
        {
            if (SelectedJob is null)
            {
                return;
            }

            Save();
            _messenger.Send(new ToastMessage("Sort job saved", ToastType.Success));
        }

        public async Task StartJob()
        {
            JobRunnerProgress = 0;
            CurrentJobRunnerTask = string.Empty;
            IsJobRunning = true;
            NotifyCommandsCanExecuteChanged();

            await Task.Run(async () =>
            {
                try
                {
                    Guard.IsNotNull(SelectedJob, nameof(SelectedJob));

                    _jobCancelTokenSource?.Cancel();
                    _jobCancelTokenSource = new CancellationTokenSource();
                    _jobCancelToken = _jobCancelTokenSource.Token;

                    var report = await _jobRunner.RunJob(SelectedJob, IsDryRun, _jobCancelToken);
                    report.ReportPath = await _reportWriter.WriteReport(report);

                    IsJobRunning = false;

                    var result = await _dialogService.ShowMessageAsync(
                        "Job Finished",
                        "Job run completed.  Would you like to open the report directory?",
                        MessageDialogStyle.AffirmativeAndNegative,
                        new MetroDialogSettings() { DefaultButtonFocus = MessageDialogResult.Affirmative });

                    if (result == MessageDialogResult.Affirmative)
                    {
                        _processService.Start(new ProcessStartInfo()
                        {
                            FileName = Path.GetDirectoryName(report.ReportPath),
                            UseShellExecute = true
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while running job name {name}.", SelectedJob?.Name);
                }
                finally
                {
                    IsJobRunning = false;
                    NotifyCommandsCanExecuteChanged();
                }
            });
        
        }

        private void CancelJob()
        {
            _jobCancelTokenSource?.Cancel();
        }

        private async Task CreateNewJob()
        {
            var result = await _dialogService.ShowInputAsync("New Sort Job", "Enter a name for the new sort job");

            if (string.IsNullOrWhiteSpace(result))
            {
                return;
            }

            var sortJob = new SortJob()
            {
                Name = result
            };

            SortJobs.Add(sortJob);
            Save();

            LoadSortJobs();
            SelectedJob = SortJobs.FirstOrDefault(x => x.Id == sortJob.Id);
        }

        private void JobRunner_CurrentTaskChanged(object? sender, string task)
        {
            _dispatcher.Invoke(() =>
            {
                CurrentJobRunnerTask = task;
            });

        }

        private void JobRunner_ProgressChanged(object? sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            _dispatcher.Invoke(() =>
            {
                JobRunnerProgress = e.ProgressPercentage;
            });
        }

        private void OpenReportsDirectory()
        {
            _processService.Start(new ProcessStartInfo()
            {
                FileName = AppConstants.PhotoSorterReportsDir,
                UseShellExecute = true
            });
        }
        private void Save()
        {
            _settings.SortJobs = SortJobs.ToArray();
        }

        private async Task ShowDestinationTransform()
        {
            await _dialogService.ShowMessageAsync("Destination Transform", GetDestinationTransform());
        }
    }
}
