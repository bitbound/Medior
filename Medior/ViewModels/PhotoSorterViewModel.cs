using MahApps.Metro.Controls.Dialogs;
using Medior.Models.PhotoSorter;
using Medior.Services.PhotoSorter;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Medior.ViewModels
{
    public interface IPhotoSorterViewModel
    {
        ICommand CreateNewJobCommand { get; }
        string CurrentJobRunnerTask { get; set; }
        ICommand DeleteJobCommand { get; }
        bool IsDryRun { get; set; }
        bool IsJobRunning { get; set; }
        int JobRunnerProgress { get; set; }
        string JobRunnerProgressPercent { get; }
        ICommand RenameJobCommand { get; }
        ICommand SaveJobCommand { get; }
        SortJob? SelectedJob { get; set; }
        ICommand ShowDestinationTransformCommand { get; }
        ObservableCollectionEx<SortJob> SortJobs { get; }
        void NotifyCommandsCanExecuteChanged();
    }

    public class PhotoSorterViewModel : ObservableObjectEx, IPhotoSorterViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly IUiDispatcher _dispatcher;
        private readonly IJobRunner _jobRunner;
        private readonly ILogger<PhotoSorterViewModel> _logger;
        private readonly IMessenger _messenger;
        private readonly IPathTransformer _pathTransformer;
        private readonly IReportWriter _reportWriter;
        private readonly ISettings _settings;
        public PhotoSorterViewModel(
            IPathTransformer pathTransformer,
            IJobRunner jobRunner,
            IReportWriter reportWriter,
            IUiDispatcher dispatcher,
            IDialogService dialogService,
            ISettings settings,
            IMessenger messenger,
            ILogger<PhotoSorterViewModel> logger)
        {
            _pathTransformer = pathTransformer;
            _jobRunner = jobRunner;
            _reportWriter = reportWriter;
            _dispatcher = dispatcher;
            _dialogService = dialogService;
            _settings = settings;
            _messenger = messenger;
            _logger = logger;

            _jobRunner.ProgressChanged += JobRunner_ProgressChanged;
            _jobRunner.CurrentTaskChanged += JobRunner_CurrentTaskChanged;

            CreateNewJobCommand = new AsyncRelayCommand(CreateNewJob);
            SaveJobCommand = new RelayCommand(SaveJob);
            RenameJobCommand = new AsyncRelayCommand(RenameSortJob);
            DeleteJobCommand = new AsyncRelayCommand(DeleteSortJob);
            ShowDestinationTransformCommand = new AsyncRelayCommand(ShowDestinationTransform);

            LoadSortJobs();
        }

        public ICommand CreateNewJobCommand { get; }

        public string CurrentJobRunnerTask
        {
            get => Get<string>() ?? "";
            set => Set(value);
        }

        public ICommand DeleteJobCommand { get; }

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

        public ICommand RenameJobCommand { get; }

        public ICommand SaveJobCommand { get; }

        public SortJob? SelectedJob
        {
            get => Get<SortJob?>();
            set
            {
                Set(value);
                OnPropertyChanged(nameof(GetIncludeExtensions));
                OnPropertyChanged(nameof(GetExcludeExtensions));
            }
        }

        public ICommand ShowDestinationTransformCommand { get; }

        public ObservableCollectionEx<SortJob> SortJobs { get; } = new();

        public async Task DeleteSortJob()
        {
            var result = await _dialogService.ShowMessageAsync(
                "Confirm Delete",
                "Are you sure you want to delete this sort job?");

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

        public string GetExcludeExtensions()
        {
            if (SelectedJob?.ExcludeExtensions is null)
            {
                return string.Empty;
            }
            return string.Join(", ", SelectedJob.ExcludeExtensions);
        }

        public string GetIncludeExtensions()
        {
            if (SelectedJob?.IncludeExtensions is null)
            {
                return string.Empty;
            }
            return string.Join(", ", SelectedJob.IncludeExtensions);
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
            OnPropertyChanged(nameof(GetIncludeExtensions));
            OnPropertyChanged(nameof(GetExcludeExtensions));
        }

        public void NotifyCommandsCanExecuteChanged()
        {
            
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

        public void SetExcludeExtensions(string extensions)
        {
            if (SelectedJob is null)
            {
                return;
            }

            SelectedJob.ExcludeExtensions = extensions.Split(",", StringSplitOptions.TrimEntries);
        }

        public void SetIncludeExtensions(string extensions)
        {
            if (SelectedJob is null)
            {
                return;
            }

            SelectedJob.IncludeExtensions = extensions.Split(",", StringSplitOptions.TrimEntries);
        }

        public async Task<JobReport> StartJob(CancellationToken cancellationToken)
        {
            Guard.IsNotNull(SelectedJob, nameof(SelectedJob));
            var report = await _jobRunner.RunJob(SelectedJob, IsDryRun, cancellationToken);
            report.ReportPath = await _reportWriter.WriteReport(report);
            return report;
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
