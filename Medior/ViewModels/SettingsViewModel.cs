using ControlzEx.Theming;
using Medior.Models;
using Medior.Services;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        private readonly IThemeSetter _themeSetter;

        public SettingsViewModel(ISettings settings, IThemeSetter themeSetter)
        {
            _settings = settings;
            _themeSetter = themeSetter;
            SetThemeCommand = new RelayCommand<AppTheme>(parameter =>
            {
                Theme = parameter;
            });
        }


        public AppTheme Theme
        {
            get => _settings.Theme;
            set
            {
                _settings.Theme = value;
                _themeSetter.SetTheme(value);
            }
        }

        public ICommand SetThemeCommand { get; }
    }
}
