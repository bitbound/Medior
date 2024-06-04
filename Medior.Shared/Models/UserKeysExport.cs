namespace Medior.Shared.Models;

public class UserKeysExport : ICloneable
{
    public UserKeysExport(byte[] publicKey, byte[] privateKey, byte[] encryptedPrivateKey)
    {
        PublicKey = publicKey;
        PrivateKey = privateKey;
        EncryptedPrivateKey = encryptedPrivateKey;
    }

    public byte[] EncryptedPrivateKey { get; private set; } = Array.Empty<byte>();
    public byte[] PrivateKey { get; private set; }
    public string PrivateKeyBase64 => Convert.ToBase64String(PrivateKey);
    public byte[] PublicKey { get; private set; }
    public string PublicKeyBase64 => Convert.ToBase64String(PublicKey);

    public object Clone()
    {
        return new UserKeysExport(PublicKey, PrivateKey, EncryptedPrivateKey);
    }
}
