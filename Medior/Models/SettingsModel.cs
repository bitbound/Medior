using Medior.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Models
{
    internal class SettingsModel
    {
        public AppTheme Theme { get; set; }

        public bool HandlePrintScreen { get; set; }
    }
}
