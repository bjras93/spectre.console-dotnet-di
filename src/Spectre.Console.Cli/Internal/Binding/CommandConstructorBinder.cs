namespace Spectre.Console.Cli;

internal static class CommandConstructorBinder
{
    public static CommandSettings CreateSettings(
        CommandValueLookup lookup,
        ConstructorInfo constructor,
        IServiceProvider provider)
    {
        if (constructor.DeclaringType == null)
        {
            throw new InvalidOperationException("Cannot create settings since constructor have no declaring type.");
        }

        var parameters = new List<object?>();
        var mapped = new HashSet<Guid>();
        foreach (var parameter in constructor.GetParameters())
        {
            if (lookup.TryGetParameterWithName(parameter.Name, out var result))
            {
                parameters.Add(result.Value);
                mapped.Add(result.Parameter.Id);
            }
            else
            {
                var value = provider.GetService(parameter.ParameterType);
                if (value == null)
                {
                    throw CommandRuntimeException.CouldNotResolveType(parameter.ParameterType);
                }

                parameters.Add(value);
            }
        }

        // Create the settings. ?? TODO why do we allow parameters on settings ??
        // if (Activator.CreateInstance(constructor.DeclaringType, parameters.ToArray()) is not CommandSettings settings)
        // {
        //     throw new InvalidOperationException("Could not create settings");
        // }
        if (provider.GetService(constructor.DeclaringType) is not CommandSettings settings)
        {
            throw new InvalidOperationException("Could not create settings");
        }

        // Try to do property injection for parameters that wasn't injected.
        foreach (var (parameter, value) in lookup)
        {
            var property = settings.Type.GetProperty(parameter.PropertyName);
            if (property == null)
            {
                continue;
            }

            if (!mapped.Contains(parameter.Id) && property.SetMethod != null)
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
}