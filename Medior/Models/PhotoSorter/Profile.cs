using System;
using System.Collections.Generic;

namespace Medior.Models.PhotoSorter
{
    public class Profile
    {
        public DateTimeOffset LastSaved { get; set; }
        public List<SortJob> SortJobs { get; init; } = new();
        public bool IsCloudSyncEnabled { get; set; }
    }
}
