namespace Spectre.Console.Cli;

/// <summary>
/// Represents a command limiter.
/// </summary>
/// <typeparam name="TSettings">The type of the settings to limit to.</typeparam>
/// <seealso cref="ICommand" />
public interface ICommandLimiter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces |
    DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] out TSettings> : ICommand
    where TSettings : CommandSettings
{
}