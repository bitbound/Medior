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

namespace Medior
{

    public partial class ShellWindow : MetroWindow
    {
        public ShellWindow()
        {
            InitializeComponent();
        }


        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            NavMenu.Items.Add(new NavMenuItem()
            {
                Icon = new PackIconOcticons() { Kind = PackIconOcticonsKind.Home },
                Label = "Home",
                NavigationType = typeof(HomeView)
            });

            



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
