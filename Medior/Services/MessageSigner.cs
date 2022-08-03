using System.Security.Cryptography;

namespace Medior.Services
{
    public interface IMessageSigner
    {
        void GenerateKeys(out RSAParameters privateKey, out RSAParameters publicKey);
        byte[] Sign(byte[] payload, RSAParameters key);
        bool Verify(byte[] payload, byte[] signature, RSAParameters key);
    }

    public class MessageSigner : IMessageSigner
    {
        public void GenerateKeys(out RSAParameters privateKey, out RSAParameters publicKey)
        {
            using var provider = new RSACryptoServiceProvider();
            privateKey = provider.ExportParameters(true);
            publicKey = provider.ExportParameters(false);
        }

        public byte[] Sign(byte[] payload, RSAParameters key)
        {
            using var provider = new RSACryptoServiceProvider();
            provider.ImportParameters(key);
            return provider.SignData(payload, "SHA256");
        }

        public bool Verify(byte[] payload, byte[] signature, RSAParameters key)
        {
            using var provider = new RSACryptoServiceProvider();
            provider.ImportParameters(key);
            return provider.VerifyData(payload, "SHA256", signature);
        }
    }
}
