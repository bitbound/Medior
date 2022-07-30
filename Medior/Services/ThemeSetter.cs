using ControlzEx.Theming;
using Medior.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Medior.Services
{
    public interface IThemeSetter
    {
        void SetTheme(AppTheme appTheme);
    }

    public class ThemeSetter : IThemeSetter
    {
        public void SetTheme(AppTheme appTheme)
        {
            switch (appTheme)
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
}
