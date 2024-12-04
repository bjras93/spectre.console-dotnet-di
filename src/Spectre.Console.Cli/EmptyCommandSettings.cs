namespace Spectre.Console.Cli;

/// <summary>
/// Represents empty settings.
/// </summary>
public sealed class EmptyCommandSettings : CommandSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyCommandSettings"/> class.
    /// </summary>
    public EmptyCommandSettings()
    : base(typeof(EmptyCommandSettings))
    {
    }
}