namespace Spectre.Console.Cli;

internal sealed class CommandValueBinder
{
    private readonly CommandValueLookup _lookup;

    public CommandValueBinder(CommandValueLookup lookup)
    {
        _lookup = lookup;
    }

    public void Bind(
        CommandParameter parameter,
        IServiceProvider provider,
        object? value)
    {
        if (parameter.ParameterKind == ParameterKind.Pair)
        {
            value = GetLookup(parameter, provider, value);
        }
        else if (parameter.ParameterKind == ParameterKind.Vector)
        {
            value = GetArray(parameter, value);
        }
        else if (parameter.ParameterKind == ParameterKind.FlagWithValue)
        {
            value = GetFlag(parameter, value);
        }

        _lookup.SetValue(parameter, value);
    }

    private object GetLookup(
        CommandParameter parameter,
        IServiceProvider provider,
        object? value)
    {
        var genericTypes = parameter.PropertyType.GenericTypeArguments;

        var multimap = (IMultiMap?)_lookup.GetValue(parameter);
        if (multimap == null)
        {
            multimap = Activator.CreateInstance(typeof(MultiMap<,>).MakeGenericType(genericTypes[0], genericTypes[1])) as IMultiMap;
            if (multimap == null)
            {
                throw new InvalidOperationException("Could not create multimap");
            }
        }

        // Create deconstructor.
        var deconstructorType = parameter.PairDeconstructor?.Type ?? typeof(DefaultPairDeconstructor);
        if (provider.GetService(deconstructorType) is not IPairDeconstructor deconstructor)
        {
            if (Activator.CreateInstance(deconstructorType) is not IPairDeconstructor activatedDeconstructor)
            {
                throw new InvalidOperationException("Could not create pair deconstructor.");
            }

            deconstructor = activatedDeconstructor;
        }

        var keyType = genericTypes[0];
        var valueType = genericTypes[1];
        var stringValue = value as string;

        // Deconstruct and add to multimap.
        var pair = deconstructor.Deconstruct(provider, keyType, valueType, stringValue);
        if (pair.Key != null)
        {
            multimap.Add(pair);
        }

        return multimap;
    }

    private object GetArray(CommandParameter parameter, object? value)
    {
        if (value is Array)
        {
            return value;
        }

        // Add a new item to the array
        var array = (Array?)_lookup.GetValue(parameter);
        Array newArray;

        var elementType = parameter.PropertyType.GetElementType();
        if (elementType == null)
        {
            throw new InvalidOperationException("Could not get property type.");
        }

        if (array == null)
        {
            newArray = Array.CreateInstance(elementType, 1);
        }
        else
        {
            newArray = Array.CreateInstance(elementType, array.Length + 1);
            array.CopyTo(newArray, 0);
        }

        newArray.SetValue(value, newArray.Length - 1);
        return newArray;
    }

    private object GetFlag(CommandParameter parameter, object? value)
    {
        var flagValue = (IFlagValue?)_lookup.GetValue(parameter);
        if (flagValue == null)
        {
            flagValue = (IFlagValue?)Activator.CreateInstance(parameter.PropertyType);
            if (flagValue == null)
            {
                throw new InvalidOperationException("Could not create flag value.");
            }
        }

        if (value != null)
        {
            // Null means set, but not with a valid value.
            flagValue.Value = value;
        }

        // If the parameter was mapped, then it's set.
        flagValue.IsSet = true;

        return flagValue;
    }
}