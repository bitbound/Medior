using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace Medior.Controls;

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
