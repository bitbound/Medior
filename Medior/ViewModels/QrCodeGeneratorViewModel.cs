using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media;

namespace Medior.ViewModels;

public partial class QrCodeGeneratorViewModel : ObservableObject
{
    private readonly IQrCodeGenerator _qrCodeGenerator;
    private readonly IDialogService _dialogs;
    private readonly IMessenger _messenger;
    private readonly ILogger<QrCodeGeneratorViewModel> _logger;


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateCodeCommand))]
    private string _inputText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveQrCodeImageCommand))]
    private ImageSource? _qrCodeImage;



    public QrCodeGeneratorViewModel(
        IQrCodeGenerator qrCodeGenerator, 
        IDialogService dialogService,
        IMessenger messenger,
        ILogger<QrCodeGeneratorViewModel> logger)
    {
        _qrCodeGenerator = qrCodeGenerator;
        _dialogs = dialogService;
        _messenger = messenger;
        _logger = logger;
    }



    private bool IsGenerateEnabled => !string.IsNullOrWhiteSpace(InputText);

    private bool IsSaveEnabled => QrCodeImage is not null;

    private Bitmap? _currentBitmap;

    [RelayCommand]
    private void Clear()
    {
        _currentBitmap?.Dispose();
        _currentBitmap = null;
        QrCodeImage = null;
        InputText = string.Empty;
    }

    [RelayCommand(CanExecute = nameof(IsGenerateEnabled))]
    private void GenerateCode()
    {
        var result = _qrCodeGenerator.GenerateCode(InputText);
        if (!result.IsSuccess)
        {
            _messenger.Send(new ToastMessage("QR code generation failed", ToastType.Warning));
            return;
        }

        _currentBitmap?.Dispose();
        _currentBitmap = result.Value;
        QrCodeImage = result.Value?.ToBitmapImage(ImageFormat.Png);
    }

    [RelayCommand(CanExecute = nameof(IsSaveEnabled))]
    private void SaveQrCodeImage()
    {
        try
        {
            if (_currentBitmap is null)
            {
                _messenger.Send(new ToastMessage("Bitmap unexpectedly null", ToastType.Warning));
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
            _currentBitmap.Save(fs, ImageFormat.Jpeg);
            _messenger.Send(new ToastMessage("Image saved", ToastType.Success));
        }
      catch (Exception ex)
        {
            _logger.LogError(ex, "Error while saving QR code.");
            _dialogs.ShowError(ex);
        }
    }
}
