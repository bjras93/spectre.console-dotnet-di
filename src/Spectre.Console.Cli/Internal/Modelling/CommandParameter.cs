namespace Spectre.Console.Cli;

internal abstract class CommandParameter : ICommandParameterInfo, ICommandParameter
{
    public Guid Id { get; }
    public ParameterKind ParameterKind { get; }
    public string? Description { get; }
    public DefaultValueAttribute? DefaultValue { get; }
    public TypeConverterAttribute? Converter { get; }
    public PairDeconstructorAttribute? PairDeconstructor { get; }
    public List<ParameterValidationAttribute> Validators { get; }
    public ParameterValueProviderAttribute? ValueProvider { get; }
    public bool Required { get; set; }
    public bool IsHidden { get; }
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type PropertyType { get; }
    public string PropertyName { get; }

    public virtual bool WantRawValue => PropertyType.IsPairDeconstructable()
        && (PairDeconstructor != null || Converter == null);

    public bool IsFlag => ParameterKind == ParameterKind.Flag;

    protected CommandParameter(
        ParameterKind parameterKind, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type propertyType,
        string propertyName, string? description, TypeConverterAttribute? converter,
        DefaultValueAttribute? defaultValue,
        PairDeconstructorAttribute? deconstructor,
        ParameterValueProviderAttribute? valueProvider,
        IEnumerable<ParameterValidationAttribute> validators, bool required, bool isHidden)
    {
        Id = Guid.NewGuid();
        ParameterKind = parameterKind;
        Description = description;
        Converter = converter;
        DefaultValue = defaultValue;
        PairDeconstructor = deconstructor;
        ValueProvider = valueProvider;
        Validators = new List<ParameterValidationAttribute>(validators ?? Array.Empty<ParameterValidationAttribute>());
        Required = required;
        IsHidden = isHidden;
        PropertyName = propertyName;
        PropertyType = propertyType;
    }

    public bool IsFlagValue()
    {
        return PropertyType.GetInterfaces().Any(i => i == typeof(IFlagValue));
    }

    public bool HaveSameBackingPropertyAs(CommandParameter other)
    {
        return CommandParameterComparer.ByBackingProperty.Equals(this, other);
    }

    public object? Get(
        CommandSettings settings)
    {
        return settings.Type.GetProperty(PropertyName)?.GetValue(settings);
    }
}