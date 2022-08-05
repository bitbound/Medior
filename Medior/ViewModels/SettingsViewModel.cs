using CommunityToolkit.Mvvm.Input;
using Medior.Models;
using System.Windows.Input;

namespace Medior.ViewModels
{
    public class SettingsViewModel
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


        public bool HandlePrintScreen
        {
            get => _settings.HandlePrintScreen;
            set => _settings.HandlePrintScreen = value;
        }

        public ICommand SetThemeCommand { get; }

        public AppTheme Theme
        {
            get => _settings.Theme;
            set
            {
                _settings.Theme = value;
                _themeSetter.SetTheme(value);
            }
        }

        public bool StartAtLogon
        {
            get => _settings.StartAtLogon;
            set => _settings.StartAtLogon = value;
        }
    }
}
