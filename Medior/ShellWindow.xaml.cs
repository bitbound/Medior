using Medior.ViewModels;
using MahApps.Metro.Controls;
using System;
using Medior.Models;

namespace Medior
{

    public partial class ShellWindow : MetroWindow
    {
        public ShellWindow()
        {
            InitializeComponent();
        }

        public ShellViewModel? ViewModel => DataContext as ShellViewModel;

        private void NavMenu_ItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs args)
        {
            if (args.InvokedItem is not AppModule navMenuItem)
            {
                return;
            }

            NavMenu.Content = Activator.CreateInstance(navMenuItem.ControlType);
        }

        private async void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            if (DataContext is ShellViewModel vm)
            {
                await vm.LoadPrivateKey();
            }
        }
    }
}
