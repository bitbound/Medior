using Medior.Shared;
using Microsoft.Extensions.Logging;
using QRCoder;
using System;
using System.Drawing;

namespace Medior.Services;

public interface IQrCodeGenerator
{
    Result<Bitmap> GenerateCode(string url);
}

public class QrCodeGenerator : IQrCodeGenerator
{
    private readonly ILogger<QrCodeGenerator> _logger;

    public QrCodeGenerator(ILogger<QrCodeGenerator> logger)
    {
        _logger = logger;
    }

    public Result<Bitmap> GenerateCode(string url)
    {
        try
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrCodeData);
            return Result.Ok(qrCode.GetGraphic(20));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while generating QR code.");
            return Result.Fail<Bitmap>(ex);
        }
    }
}
