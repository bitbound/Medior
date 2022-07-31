using Medior.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Models
{
    public class SettingsModel
    {
        public AppTheme Theme { get; set; } = AppTheme.Dark;
        public string ServerUri { get; set; } = string.Empty;
        public bool HandlePrintScreen { get; set; } = true;
        public bool StartAtLogon { get; set; } = true;
    }
}
