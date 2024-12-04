namespace Spectre.Console.Cli;

/// <summary>
/// Represents a pair deconstructor.
/// </summary>
internal interface IPairDeconstructor
{
    /// <summary>
    /// Deconstructs the specified value into its components.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    /// <param name="keyType">The key type.</param>
    /// <param name="valueType">The value type.</param>
    /// <param name="value">The value to deconstruct.</param>
    /// <returns>A deconstructed value.</returns>
    (object? Key, object? Value) Deconstruct(
        IServiceProvider provider,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        Type keyType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        Type valueType,
        string? value);
}