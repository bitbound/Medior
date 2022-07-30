using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Models.Messages
{
    public class LoaderUpdate
    {
        public bool IsShown { get; init; }
        public string Text { get; init; } = string.Empty;
        public double LoaderProgress { get; init; }
        public LoaderType Type { get; init; }
    }

    public enum LoaderType
    {
        Unknown,
        Message,
        Progress
    }
}
