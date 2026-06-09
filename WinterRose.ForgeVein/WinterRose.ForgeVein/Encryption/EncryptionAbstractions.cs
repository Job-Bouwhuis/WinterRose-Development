using WinterRose.ForgeVein.Networking.Session;

namespace WinterRose.ForgeVein.Networking.Encryption;

public enum EncryptionAlgorithm
{
    None,
    Aes256Gcm,
    ChaCha20Poly1305,
    Hybrid
}

public sealed record EncryptionState(
    EncryptionAlgorithm Algorithm,
    bool IsEnabled,
    DateTime? InitializedAt,
    IDictionary<string, object?> Metadata
);

public interface IEncryptionPipeline
{
    EncryptionAlgorithm SupportedAlgorithm { get; }

    ValueTask<ReadOnlyMemory<byte>> EncryptAsync(
        ReadOnlyMemory<byte> plaintext, 
        ISessionConnection session, 
        CancellationToken cancellationToken = default);

    ValueTask<ReadOnlyMemory<byte>> DecryptAsync(
        ReadOnlyMemory<byte> ciphertext, 
        ISessionConnection session, 
        CancellationToken cancellationToken = default);
}

public interface IEncryptionStateTracker
{
    EncryptionState GetState(ISessionConnection session);
    void SetState(ISessionConnection session, EncryptionState state);
    void InitializeEncryption(ISessionConnection session, EncryptionAlgorithm algorithm);
    void DisableEncryption(ISessionConnection session);
}

public interface IKeyExchangeProtocol
{
    ValueTask<ReadOnlyMemory<byte>> GeneratePublicKeyAsync(CancellationToken cancellationToken = default);
    ValueTask<ReadOnlyMemory<byte>> DeriveSessionKeyAsync(
        ReadOnlyMemory<byte> publicKey, 
        CancellationToken cancellationToken = default);
}
