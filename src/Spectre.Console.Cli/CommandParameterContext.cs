namespace Spectre.Console.Cli;

/// <summary>
/// Represents a context for <see cref="ICommandParameterInfo"/> related operations.
/// </summary>
public sealed class CommandParameterContext
{
    /// <summary>
    /// Gets the parameter.
    /// </summary>
    public ICommandParameterInfo Parameter { get; }

    /// <summary>
    /// Gets the service provider.
    /// </summary>
    public IServiceProvider Provider { get; }

    /// <summary>
    /// Gets tje parameter value.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandParameterContext"/> class.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="provider">The service provider.</param>
    /// <param name="value">The parameter value.</param>
    public CommandParameterContext(ICommandParameterInfo parameter, IServiceProvider provider, object? value)
    {
        Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        Provider = provider;
        Value = value;
    }
}