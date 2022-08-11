using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;

namespace Medior.Services
{
    public interface IMessageSigner
    {
        byte[] EncryptedPrivateKey { get; }
        RSAParameters PrivateKey { get; }
        RSAParameters PublicKey { get; }

        void GenerateKeys(string password);
        bool ImportPrivateKey(string password, byte[] encryptedKey);
        byte[] Sign(byte[] payload);
        bool Verify(byte[] payload, byte[] signature);
    }

    public class MessageSigner : IMessageSigner
    {
        private readonly PbeParameters _pbeParameters = new(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA512, 5_000);
        private readonly ILogger<MessageSigner> _logger;
        private readonly RSA _rsa = RSA.Create();

        public MessageSigner(ILogger<MessageSigner> logger)
        {
            _logger = logger;
        }

        public RSAParameters PublicKey { get; private set; }
        public RSAParameters PrivateKey { get; private set; }
        public byte[] EncryptedPrivateKey { get; private set; } = Array.Empty<byte>();

        public void GenerateKeys(string password)
        {
            EncryptedPrivateKey = _rsa.ExportEncryptedPkcs8PrivateKey(password, _pbeParameters);
            PrivateKey = _rsa.ExportParameters(true);
            PublicKey = _rsa.ExportParameters(false);
        }


        public byte[] Sign(byte[] payload)
        {
            return _rsa.SignData(payload, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
        }

        public bool ImportPrivateKey(string password, byte[] encryptedKey)
        {
            try
            {
                _rsa.ImportEncryptedPkcs8PrivateKey(password, encryptedKey, out var bytesRead);
                PrivateKey = _rsa.ExportParameters(true);
                PublicKey = _rsa.ExportParameters(false);
                EncryptedPrivateKey = encryptedKey;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while importing private key.");
                return false;
            }
        }


        public bool Verify(byte[] payload, byte[] signature)
        {
            return _rsa.VerifyData(payload, signature, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
        }
    }
}
