using WinterRose.ForgeVein.Networking.Packets;

namespace WinterRose.ForgeVein.Networking.Validation;

public interface IValidationResult
{
    bool IsValid { get; }
    IReadOnlyList<string> Errors { get; }
}

public interface IValidator<in T>
{
    IValidationResult Validate(T instance);
}

public interface IValidationRegistry
{
    bool TryGetValidator(Type packetType, out object? validator);
    void Register<TPacket>(IValidator<TPacket> validator);
}

public interface IValidationPipeline
{
    IValidationResult ValidatePacket<TPacket>(TPacket packet, PacketMetadata metadata);
}

public interface IRuleBuilder<T, TProperty>
{
    IRuleBuilder<T, TProperty> MinLength(int length);
    IRuleBuilder<T, TProperty> NotEmpty();
    IRuleBuilder<T, TProperty> WithMessage(string message);
}

public interface IValidationBuilder<T>
{
    IRuleBuilder<T, TProperty> RuleFor<TProperty>(Func<T, TProperty> accessor);
}

public interface IValidationDefinition<T> : IValidator<T>
{
    void Define(IValidationBuilder<T> builder);
}
