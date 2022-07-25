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

        public SettingsViewModel(ISettings settings)
        {
            _settings = settings;
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
                switch (value)
                {
                    case AppTheme.Default:
                        ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
                        ThemeManager.Current.SyncTheme();
                        break;
                    case AppTheme.Light:
                        ThemeManager.Current.ChangeTheme(Application.Current, "Light.Blue");
                        break;
                    case AppTheme.Dark:
                        ThemeManager.Current.ChangeTheme(Application.Current, "Dark.Blue");
                        break;
                    default:
                        break;
                }
            }
        }

        public ICommand SetThemeCommand { get; }
    }
}
