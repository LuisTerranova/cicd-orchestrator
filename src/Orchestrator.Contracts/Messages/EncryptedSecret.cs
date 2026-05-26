namespace Orchestrator.Contracts.Messages;

public sealed record EncryptedSecret(string Nonce, string Ciphertext, string Tag);
