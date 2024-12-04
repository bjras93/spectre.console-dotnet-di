namespace Spectre.Console.Cli;

internal sealed class CommandArgument : CommandParameter, ICommandArgument
{
    public string Value { get; }
    public int Position { get; set; }

    public CommandArgument(
        ParameterKind parameterKind,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        Type propertyType,
        string propertyName,
        string? description,
        TypeConverterAttribute? converter,
        DefaultValueAttribute? defaultValue,
        CommandArgumentAttribute argument,
        ParameterValueProviderAttribute? valueProvider,
        IEnumerable<ParameterValidationAttribute> validators)
            : base(parameterKind, propertyType, propertyName, description, converter, defaultValue,
                  null, valueProvider, validators, argument.IsRequired, false)
    {
        Value = argument.ValueName;
        Position = argument.Position;
    }
}