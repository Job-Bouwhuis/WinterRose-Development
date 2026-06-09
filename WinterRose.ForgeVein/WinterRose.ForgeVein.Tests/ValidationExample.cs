using WinterRose.ForgeVein.Networking.Validation;
using WinterRose.ForgeVein.Networking.Defaults;

namespace WinterRose.ForgeVein.Tests;

/// <summary>
/// Example demonstrating the fluent validation API.
/// </summary>
public class ValidationExample
{
    public static void RunExample()
    {
        Console.WriteLine("=== WinterRose.ForgeVein Validation Example ===\n");

        // Create a sample packet class
        var userPacket = new UserPacket { Username = "jo", Email = "invalid-email" };

        // Create a validation definition
        var validationDefinition = new UserPacketValidator();

        // Validate the packet
        var result = validationDefinition.Validate(userPacket);

        Console.WriteLine($"Validation Result: {(result.IsValid ? "VALID" : "INVALID")}");
        if (!result.IsValid)
        {
            Console.WriteLine("Errors:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  - {error}");
            }
        }
        else
        {
            Console.WriteLine("No validation errors.");
        }

        Console.WriteLine();

        // Test with valid data
        var validUserPacket = new UserPacket { Username = "john_doe", Email = "john@example.com" };
        var validResult = validationDefinition.Validate(validUserPacket);

        Console.WriteLine($"Valid Packet Validation Result: {(validResult.IsValid ? "VALID" : "INVALID")}");
        if (!validResult.IsValid)
        {
            Console.WriteLine("Errors:");
            foreach (var error in validResult.Errors)
            {
                Console.WriteLine($"  - {error}");
            }
        }
        else
        {
            Console.WriteLine("No validation errors.");
        }
    }
}

public class UserPacket
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UserPacketValidator : IValidationDefinition<UserPacket>
{
    private readonly ValidationBuilder<UserPacket> builder = new();

    public UserPacketValidator()
    {
        Define(builder);
    }

    public void Define(IValidationBuilder<UserPacket> builder)
    {
        builder.RuleFor(x => x.Username)
            .MinLength(5)
            .WithMessage("Username must be at least 5 characters long.");

        builder.RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.");
    }

    public IValidationResult Validate(UserPacket instance)
    {
        return builder.Validate(instance);
    }
}
