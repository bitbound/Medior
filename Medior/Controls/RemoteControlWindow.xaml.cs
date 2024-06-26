﻿using MahApps.Metro.Controls;
using Medior.ViewModels;
using System;
using System.Windows;
using System.Windows.Data;

namespace Medior.Controls;

/// <summary>
/// Interaction logic for RemoteControlWindow.xaml
/// </summary>
public partial class RemoteControlWindow : MetroWindow
{
    private readonly Guid _streamId;

    public RemoteControlWindow()
    {
        InitializeComponent();
    }

    public RemoteControlWindow(Guid streamId)
    {
        InitializeComponent();
        _streamId = streamId;
    }

    private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is RemoteControlViewModel vm)
        {
            vm.StreamId = _streamId;
            ViewerMediaElement.Play();
        }
    }

    private void ViewerMediaElement_SourceUpdated(object sender, DataTransferEventArgs e)
    {
        ViewerMediaElement.Play();
    }
}
