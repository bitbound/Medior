using Medior.Controls;
using Medior.ViewModels;
using Medior.Views;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using Medior.Models;
using Medior.Native;

namespace Medior
{

    public partial class ShellWindow : MetroWindow
    {
        public ShellWindow()
        {
            InitializeComponent();
            PrintScreenHotkey.Set();
        }


        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var appModules = StaticServiceProvider.Instance.GetServices<AppModule>();

            foreach (var module in appModules)
            {
                NavMenu.Items.Add(new NavMenuItem()
                {
                    Icon = module.Icon,
                    Label = module.Label,
                    NavigationType = module.ControlType
                });
            }


            NavMenu.OptionsItems.Add(new NavMenuItem()
            {
                Icon = new PackIconOcticons() { Kind = PackIconOcticonsKind.Question },
                Label = "About",
                NavigationType = typeof(AboutView)
            });

            NavMenu.OptionsItems.Add(new NavMenuItem()
            {
                Icon = new PackIconOcticons() { Kind = PackIconOcticonsKind.Settings },
                Label = "Settings",
                NavigationType = typeof(SettingsView)
            });

            NavMenu.SelectedIndex = 0;
        }

        private void NavMenu_ItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs args)
        {
            if (args.InvokedItem is not NavMenuItem navMenuItem)
            {
                return;
            }

            NavMenu.Content = Activator.CreateInstance(navMenuItem.NavigationType);
        }
    }
}
