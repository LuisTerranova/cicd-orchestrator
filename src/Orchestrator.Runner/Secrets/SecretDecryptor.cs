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
        throw new NotImplementedException();
    }
}
