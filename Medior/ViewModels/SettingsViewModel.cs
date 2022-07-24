using Medior.Models;
using Medior.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Medior.ViewModels
{
    internal interface ISettingsViewModel
    {
        AppTheme Theme { get; set; }
        ICommand SetThemeCommand { get; }
    }

    internal class SettingsViewModel : ISettingsViewModel
    {
        private readonly ISettings _settings;

        public SettingsViewModel(ISettings settings)
        {
            _settings = settings;
        }


        public AppTheme Theme
        {
            get => _settings.Theme;
            set => _settings.Theme = value;
        }

        public ICommand SetThemeCommand => throw new NotImplementedException();
    }
}
