using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Medior.Controls
{
    /// <summary>
    /// Interaction logic for FlyoutsHarness.xaml
    /// </summary>
    public partial class FlyoutsHarness : FlyoutsControl
    {
        private readonly IMessenger _messeger;

        public FlyoutsHarness()
        {
            InitializeComponent();

            _messeger = StaticServiceProvider.Instance.GetRequiredService<IMessenger>();
        }

        private void FlyoutsControl_Loaded(object sender, RoutedEventArgs e)
        {
            _messeger.Register<GenericMessage<FlyoutRequestKind>>(this, HandleFlyoutRequest);
        }

        private void HandleFlyoutRequest(object recipient, GenericMessage<FlyoutRequestKind> message)
        {
            switch (message.Value)
            {
                case FlyoutRequestKind.PhotoSorterDestinationVariables:
                    DestinationVariablesFlyout.IsOpen = true;
                    break;
                default:
                    break;
            }
        }
    }
}
