namespace Spectre.Console.Cli;

internal sealed class CommandOption : CommandParameter, ICommandOption
{
    public IReadOnlyList<string> LongNames { get; }
    public IReadOnlyList<string> ShortNames { get; }
    public string? ValueName { get; }
    public bool ValueIsOptional { get; }
    public bool IsShadowed { get; set; }

    public CommandOption(
        ParameterKind parameterKind,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        Type propertyType,
        string propertyName,
        string? description,
        TypeConverterAttribute? converter,
        PairDeconstructorAttribute? deconstructor,
        CommandOptionAttribute optionAttribute,
        ParameterValueProviderAttribute? valueProvider,
        IEnumerable<ParameterValidationAttribute> validators,
        DefaultValueAttribute? defaultValue,
        bool valueIsOptional)
            : base(parameterKind, propertyType, propertyName, description, converter,
                  defaultValue, deconstructor, valueProvider, validators, false, optionAttribute.IsHidden)
    {
        LongNames = optionAttribute.LongNames;
        ShortNames = optionAttribute.ShortNames;
        ValueName = optionAttribute.ValueName;
        ValueIsOptional = valueIsOptional;
    }

    public string GetOptionName()
    {
        return LongNames.Count > 0 ? LongNames[0] : ShortNames[0];
    }
}