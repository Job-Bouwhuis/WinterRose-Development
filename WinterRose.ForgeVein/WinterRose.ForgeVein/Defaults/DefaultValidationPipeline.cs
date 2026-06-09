using WinterRose.ForgeVein.Networking.Packets;
using WinterRose.ForgeVein.Networking.Validation;

namespace WinterRose.ForgeVein.Networking.Defaults;

public sealed class DefaultValidationPipeline : IValidationPipeline
{
    private readonly IValidationRegistry registry;

    public DefaultValidationPipeline(IValidationRegistry registry)
    {
        this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public IValidationResult ValidatePacket<TPacket>(TPacket packet, PacketMetadata metadata)
    {
        if (packet == null)
            return ValidationResult.Failure(new[] { "Packet is null." });

        // Skip validation for hotpath packets
        if (metadata.Reliability == ReliabilityMode.HotPath)
            return ValidationResult.Success();

        // Skip validation if not required
        if (!metadata.RequiresValidation)
            return ValidationResult.Success();

        // Check if a validator is registered for this packet type
        if (registry.TryGetValidator(typeof(TPacket), out var validator) && validator is IValidator<TPacket> typed)
        {
            return typed.Validate(packet);
        }

        // No validator registered, pass validation
        return ValidationResult.Success();
    }
}
