using System.Collections.Concurrent;
using System.Linq.Expressions;
using WinterRose.ForgeVein.Networking.Packets;
using WinterRose.ForgeVein.Networking.Validation;

namespace WinterRose.ForgeVein.Networking.Defaults;

public sealed class ValidationResult : IValidationResult
{
    public ValidationResult(bool isValid, IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public bool IsValid { get; }
    public IReadOnlyList<string> Errors { get; }

    public static ValidationResult Success() => new(true, Array.Empty<string>());
    public static ValidationResult Failure(IEnumerable<string> errors) => new(false, errors.ToArray());
}

public sealed class DefaultValidationRegistry : IValidationRegistry
{
    private readonly ConcurrentDictionary<Type, object> validators = new();

    public bool TryGetValidator(Type packetType, out object? validator)
    {
        if (validators.TryGetValue(packetType, out var found))
        {
            validator = found;
            return true;
        }

        validator = null;
        return false;
    }

    public void Register<TPacket>(IValidator<TPacket> validator)
    {
        if (!validators.TryAdd(typeof(TPacket), validator))
            throw new InvalidOperationException($"Validator already registered for {typeof(TPacket).Name}");
    }
}

public sealed class ValidationBuilder<T> : IValidationBuilder<T>
{
    private readonly List<ValidationRule<T>> rules = new();

    public IRuleBuilder<T, TProperty> RuleFor<TProperty>(Func<T, TProperty> accessor)
    {
        if (accessor == null) throw new ArgumentNullException(nameof(accessor));
        var rule = new ValidationRule<T>(accessor);
        rules.Add(rule);
        return new RuleBuilder<T, TProperty>(rule);
    }

    public IValidationResult Validate(T instance)
    {
        List<string> errors = new();
        foreach (var rule in rules)
        {
            var result = rule.Evaluate(instance);
            if (!result.IsValid)
                errors.AddRange(result.Errors);
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }

    private sealed class RuleBuilder<TModel, TProperty> : IRuleBuilder<TModel, TProperty>
    {
        private readonly ValidationRule<TModel> rule;

        public RuleBuilder(ValidationRule<TModel> rule) => this.rule = rule;

        public IRuleBuilder<TModel, TProperty> MinLength(int length)
        {
            rule.AddCheck(value => value is string s && s.Length >= length, $"Minimum length is {length}.");
            return this;
        }

        public IRuleBuilder<TModel, TProperty> NotEmpty()
        {
            rule.AddCheck(value => value is not null && (!string.IsNullOrWhiteSpace(value.ToString())), "Value must not be empty.");
            return this;
        }

        public IRuleBuilder<TModel, TProperty> WithMessage(string message)
        {
            rule.OverrideMessage(message);
            return this;
        }
    }

    private sealed class ValidationRule<TModel>
    {
        private readonly Func<TModel, object?> accessor;
        private readonly List<(Func<object?, bool> check, string message)> checks = new();
        private string? overrideMessage;

        public ValidationRule(Delegate accessor)
        {
            this.accessor = model => accessor.DynamicInvoke(model);
        }

        public void AddCheck(Func<object?, bool> check, string message)
        {
            checks.Add((check, message));
        }

        public void OverrideMessage(string message)
        {
            overrideMessage = message;
        }

        public ValidationResult Evaluate(TModel model)
        {
            object? value = accessor(model);
            List<string> errors = new();
            foreach (var (check, message) in checks)
            {
                if (!check(value))
                    errors.Add(overrideMessage ?? message);
            }

            return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
        }
    }
}

public sealed class ValidationPipeline : IValidationPipeline
{
    private readonly IValidationRegistry registry;

    public ValidationPipeline(IValidationRegistry registry)
    {
        this.registry = registry;
    }

    public IValidationResult ValidatePacket<TPacket>(TPacket packet, PacketMetadata metadata)
    {
        if (!metadata.RequiresValidation)
            return ValidationResult.Success();

        if (registry.TryGetValidator(typeof(TPacket), out var validator) && validator is IValidator<TPacket> typed)
        {
            return typed.Validate(packet);
        }

        return ValidationResult.Success();
    }
}
