using MahApps.Metro.Controls;
using Medior.Shared.Dtos;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;

namespace Medior.Controls;

/// <summary>
/// Interaction logic for QrCodeWindow.xaml
/// </summary>
public partial class QrCodeWindow : MetroWindow
{
    private readonly Bitmap? _qrCode;
    private readonly string _title = string.Empty;
    private readonly string _url = string.Empty;

    public QrCodeWindow()
    {
        InitializeComponent();
    }

    private void HandleClipboardReady(object recipient, ClipboardReadyDto message)
    {
        if (_url.Contains(message.ReceiptToken))
        {
            WeakReferenceMessenger.Default.Unregister<ClipboardReadyDto>(this);
            Close();
        }
    }

    public QrCodeWindow(Bitmap? value, string title, string url)
    {
        InitializeComponent();
        _qrCode = value;
        _title = title;
        _url = url;
        WeakReferenceMessenger.Default.Register<ClipboardReadyDto>(this, HandleClipboardReady);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Title = _title;
        HeaderText.Text = _title;
        QrImage.Source = _qrCode?.ToBitmapImage(ImageFormat.Png);
        QrUrlTextBox.Text = _url;
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(_url);
        CopyButton.ShowTooltip("Copied to clipboard");
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_qrCode is null)
        {
            SaveButton.ShowTooltip("Error: Bitmap is null");
            return;
        }

        var sfd = new SaveFileDialog()
        {
            Filter = "Image Files (*.jpg)|*.jpg",
            AddExtension = true,
            DefaultExt = ".jpg",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
        };

        _ = sfd.ShowDialog();
        if (string.IsNullOrWhiteSpace(sfd.FileName))
        {
            return;
        }

        using var fs = new FileStream(sfd.FileName, FileMode.Create);
        _qrCode.Save(fs, ImageFormat.Jpeg);
        SaveButton.ShowTooltip("Image saved");
    }
}
