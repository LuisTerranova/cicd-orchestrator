using System.Security.Cryptography;
using System.Text;
using Orchestrator.Contracts.Messages;

namespace Orchestrator.Runner.Secrets;

public sealed class SecretDecryptor
{
    private readonly byte[] _key;

    public SecretDecryptor(string base64Key)
    {
        _key = Convert.FromBase64String(base64Key);
    }

    public string Decrypt(EncryptedSecret encrypted)
    {
        var nonce = Convert.FromBase64String(encrypted.Nonce);
        var ciphertext = Convert.FromBase64String(encrypted.Ciphertext);
        var tag = Convert.FromBase64String(encrypted.Tag);

        var plaintext = new byte[ciphertext.Length];
        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }
}
