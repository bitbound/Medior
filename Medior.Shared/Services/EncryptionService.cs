using Medior.Shared.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;

namespace Medior.Shared.Services
{
    public interface IEncryptionService
    {
        UserKeysExport GenerateKeys(string password);
        Result<UserKeysExport> ImportPrivateKey(string password, byte[] encryptedKey);
        void ImportPublicKey(byte[] publicBytes);

        byte[] Sign(byte[] payload);
        bool Verify(byte[] payload, byte[] signature);
    }

    public class EncryptionService : IEncryptionService
    {
        private readonly ILogger<EncryptionService> _logger;
        private readonly PbeParameters _pbeParameters = new(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA512, 5_000);
        private RSA _rsa = RSA.Create();

        public EncryptionService(ILogger<EncryptionService> logger)
        {
            _logger = logger;
        }

        public UserKeysExport GenerateKeys(string password)
        {
            _rsa.Dispose();
            _rsa = RSA.Create();
            var encryptedPrivateKey = _rsa.ExportEncryptedPkcs8PrivateKey(password, _pbeParameters);
            var privateKey = _rsa.ExportRSAPrivateKey();
            var publicKey = _rsa.ExportRSAPublicKey();
            return new UserKeysExport(publicKey, privateKey, encryptedPrivateKey);
        }


        public Result<UserKeysExport> ImportPrivateKey(string password, byte[] encryptedKey)
        {
            try
            {
                _rsa.ImportEncryptedPkcs8PrivateKey(password, encryptedKey, out var bytesRead);
                var privateKey = _rsa.ExportRSAPrivateKey();
                var publicKey = _rsa.ExportRSAPublicKey();
                var encryptedPrivateKey = encryptedKey;
                var keys = new UserKeysExport(publicKey, privateKey, encryptedPrivateKey);
                return Result.Ok(keys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while importing private key.");
                return Result.Fail<UserKeysExport>(ex);
            }
        }

        public void ImportPublicKey(byte[] publicKeyBytes)
        {
            _rsa.ImportRSAPublicKey(publicKeyBytes, out _);
        }

        public byte[] Sign(byte[] payload)
        {
            return _rsa.SignData(payload, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
        }
        public bool Verify(byte[] payload, byte[] signature)
        {
            return _rsa.VerifyData(payload, signature, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
        }
    }
}
