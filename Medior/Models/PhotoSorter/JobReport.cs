using System.Collections.Generic;

namespace Medior.Models.PhotoSorter
{
    public class JobReport
    {
        public string JobName { get; init; } = string.Empty;
        public SortOperation Operation { get; init; }
        public List<OperationResult> Results { get; } = new();
        public bool DryRun { get; init; }
        public string ReportPath { get; init; } = string.Empty;
    }
}
