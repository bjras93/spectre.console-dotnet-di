namespace Spectre.Console.Cli;

/// <summary>
/// Base class for command settings.
/// </summary>
public abstract class CommandSettings
{
    /// <summary>
    /// Gets the type of the <see cref="CommandSettings"/> instance.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces |
    DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type Type { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandSettings"/> class.
    /// </summary>
    /// <param name="type">Type of the <see cref="CommandSettings"/> instance.</param>
    public CommandSettings(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces |
    DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)]
        Type type)
    {
        Type = type;
    }

    /// <summary>
    /// Performs validation of the settings.
    /// </summary>
    /// <returns>The validation result.</returns>
    public virtual ValidationResult Validate()
    {
        return ValidationResult.Success();
    }
}