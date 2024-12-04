namespace Spectre.Console.Cli;

internal static class CommandPropertyBinder
{
    public static CommandSettings CreateSettings(
        CommandValueLookup lookup,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type settingsType,
        IServiceProvider provider)
    {
        var settings = CreateSettings(provider, settingsType);
        foreach (var (parameter, value) in lookup)
        {
            var property = settings.Type.GetProperty(parameter.PropertyName);
            if (property == null)
            {
                continue;
            }

            if (value != default)
            {
                property.SetValue(settings, value);
            }
        }

        // Validate the settings.
        var validationResult = settings.Validate();
        if (!validationResult.Successful)
        {
            throw CommandRuntimeException.ValidationFailed(validationResult);
        }

        return settings;
    }

    private static CommandSettings CreateSettings(
        IServiceProvider provider,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type settingsType)
    {
        if (provider.GetService(settingsType) is CommandSettings settings)
        {
            return settings;
        }

        if (Activator.CreateInstance(settingsType) is CommandSettings instance)
        {
            return instance;
        }

        throw CommandParseException.CouldNotCreateSettings(settingsType);
    }
}