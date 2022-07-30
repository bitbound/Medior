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
        }

        private void NavMenu_ItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs args)
        {
            if (args.InvokedItem is not AppModule navMenuItem)
            {
                return;
            }

            NavMenu.Content = Activator.CreateInstance(navMenuItem.ControlType);
        }
    }
}
