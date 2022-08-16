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

        UserKeysExport RestoreState();

        void SaveState();

        byte[] Sign(byte[] payload);
        bool Verify(byte[] payload, byte[] signature);
    }

    public class EncryptionService : IEncryptionService
    {
        private readonly ILogger<EncryptionService> _logger;
        private readonly PbeParameters _pbeParameters = new(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA512, 5_000);
        private UserKeysExport? _backupKeys;
        private RSAParameters? _backupParams;
        private UserKeysExport? _currentKeys;
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
            _currentKeys = new UserKeysExport(publicKey, privateKey, encryptedPrivateKey);
            return _currentKeys;
        }


        public Result<UserKeysExport> ImportPrivateKey(string password, byte[] encryptedKey)
        {
            try
            {
                _rsa.ImportEncryptedPkcs8PrivateKey(password, encryptedKey, out var bytesRead);
                var privateKey = _rsa.ExportRSAPrivateKey();
                var publicKey = _rsa.ExportRSAPublicKey();
                var encryptedPrivateKey = encryptedKey;
                _currentKeys = new UserKeysExport(publicKey, privateKey, encryptedPrivateKey);
                return Result.Ok(_currentKeys);
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

        public UserKeysExport RestoreState()
        {
            if (_backupKeys is null || !_backupParams.HasValue)
            {
                throw new Exception("No backup state to restore.");
            }

            _rsa.ImportParameters(_backupParams.Value);
            _currentKeys = _backupKeys;

            _backupKeys = null;
            _backupParams = null;

            return _currentKeys;
        }

        public void SaveState()
        {
            if (_currentKeys is null)
            {
                throw new Exception("No current keys to save.");
            }

            _backupParams = _rsa.ExportParameters(true);
            _backupKeys = (UserKeysExport)_currentKeys.Clone();
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
