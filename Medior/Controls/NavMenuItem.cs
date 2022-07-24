using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Medior.Controls
{
    internal class NavMenuItem : HamburgerMenuIconItem
    {
        public static readonly DependencyProperty NavigationDestinationProperty = DependencyProperty.Register(
          nameof(NavigationDestination), typeof(Uri), typeof(NavMenuItem), new PropertyMetadata(default(Uri)));

        public Uri NavigationDestination
        {
            get => (Uri)GetValue(NavigationDestinationProperty);
            set => SetValue(NavigationDestinationProperty, value);
        }

        public static readonly DependencyProperty NavigationTypeProperty = DependencyProperty.Register(
          nameof(NavigationType), typeof(Type), typeof(NavMenuItem), new PropertyMetadata(default(Type)));

        public Type NavigationType
        {
            get => (Type)GetValue(NavigationTypeProperty);
            set => SetValue(NavigationTypeProperty, value);
        }

        public bool IsNavigation => NavigationDestination is not null;
    }
}
