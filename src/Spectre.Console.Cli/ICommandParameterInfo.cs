namespace Spectre.Console.Cli;

/// <summary>
/// Represents a command parameter.
/// </summary>
public interface ICommandParameterInfo
{
    /// <summary>
    /// Gets the property name.
    /// </summary>
    /// <value>The property name.</value>
    public string PropertyName { get; }

    /// <summary>
    /// Gets the property type.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type PropertyType { get; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    /// <value>The description.</value>
    public string? Description { get; }
}