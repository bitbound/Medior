using System.Collections.Generic;

namespace Medior.Models.PhotoSorter;

public class JobReport
{
    public string JobName { get; set; } = string.Empty;
    public SortOperation Operation { get; set; }
    public List<OperationResult> Results { get; } = new();
    public bool DryRun { get; set; }
    public string ReportPath { get; set; } = string.Empty;
}
