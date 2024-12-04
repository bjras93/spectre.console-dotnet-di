namespace Spectre.Console.Cli;

internal static class CommandValueResolver
{
    public static CommandValueLookup GetParameterValues(CommandTree? tree, IServiceProvider provider)
    {
        var lookup = new CommandValueLookup();
        var binder = new CommandValueBinder(lookup);

        CommandValidator.ValidateRequiredParameters(tree);

        while (tree != null)
        {
            // Process unmapped parameters.
            foreach (var parameter in tree.Unmapped)
            {
                // Got a value provider?
                if (parameter.ValueProvider != null)
                {
                    var context = new CommandParameterContext(parameter, provider, null);
                    if (parameter.ValueProvider.TryGetValue(context, out var result))
                    {
                        result = ConvertValue(provider, lookup, binder, parameter, result);

                        lookup.SetValue(parameter, result);
                        CommandValidator.ValidateParameter(parameter, lookup, provider);
                        continue;
                    }
                }

                if (parameter.IsFlagValue())
                {
                    // Set the flag value to an empty, not set instance.
                    var instance = Activator.CreateInstance(parameter.PropertyType);
                    lookup.SetValue(parameter, instance);
                }
                else
                {
                    // Is this an option with a default value?
                    if (parameter.DefaultValue != null)
                    {
                        var value = parameter.DefaultValue?.Value;
                        value = ConvertValue(provider, lookup, binder, parameter, value);

                        binder.Bind(parameter, provider, value);
                        CommandValidator.ValidateParameter(parameter, lookup, provider);
                    }
                    else if (Nullable.GetUnderlyingType(parameter.PropertyType) != null ||
                             !parameter.PropertyType.IsValueType)
                    {
                        lookup.SetValue(parameter, null);
                    }
                }
            }

            // Process mapped parameters.
            foreach (var mapped in tree.Mapped)
            {
                if (mapped.Parameter.WantRawValue)
                {
                    // Just try to assign the raw value.
                    binder.Bind(mapped.Parameter, provider, mapped.Value);
                }
                else
                {
                    if (mapped.Parameter.IsFlagValue() && mapped.Value == null)
                    {
                        if (mapped.Parameter is CommandOption option && option.DefaultValue != null)
                        {
                            // Set the default value.
                            binder.Bind(mapped.Parameter, provider, option.DefaultValue?.Value);
                        }
                        else
                        {
                            // Set the flag but not the value.
                            binder.Bind(mapped.Parameter, provider, null);
                        }
                    }
                    else
                    {
                        object? value;
                        var converter = GetConverter(lookup, binder, provider, mapped.Parameter);
                        var mappedValue = mapped.Value ?? string.Empty;
                        try
                        {
                            value = converter.ConvertFrom(mappedValue);
                        }
                        catch (Exception exception) when (exception is not CommandRuntimeException)
                        {
                            throw CommandRuntimeException.ConversionFailed(mapped, converter.TypeConverter, exception);
                        }

                        // Assign the value to the parameter.
                        binder.Bind(mapped.Parameter, provider, value);
                    }
                }

                // Got a value provider?
                if (mapped.Parameter.ValueProvider != null)
                {
                    var context = new CommandParameterContext(mapped.Parameter, provider, mapped.Value);
                    if (mapped.Parameter.ValueProvider.TryGetValue(context, out var result))
                    {
                        lookup.SetValue(mapped.Parameter, result);
                    }
                }

                CommandValidator.ValidateParameter(mapped.Parameter, lookup, provider);
            }

            tree = tree.Next;
        }

        return lookup;
    }

    private static object? ConvertValue(
        IServiceProvider provider,
        CommandValueLookup lookup,
        CommandValueBinder binder,
        CommandParameter parameter,
        object? result)
    {
        if (result != null && result.GetType() != parameter.PropertyType)
        {
            var converter = GetConverter(lookup, binder, provider, parameter);
            result = result is Array array ? ConvertArray(array, converter) : converter.ConvertFrom(result);
        }

        return result;
    }

    private static Array ConvertArray(Array sourceArray, SmartConverter converter)
    {
        Array? targetArray = null;
        for (var i = 0; i < sourceArray.Length; i++)
        {
            var item = sourceArray.GetValue(i);
            if (item != null)
            {
                var converted = converter.ConvertFrom(item);
                if (converted != null)
                {
                    targetArray ??= Array.CreateInstance(converted.GetType(), sourceArray.Length);
                    targetArray.SetValue(converted, i);
                }
            }
        }

        return targetArray ?? sourceArray;
    }

    [SuppressMessage("Style", "IDE0019:Use pattern matching", Justification = "It's OK")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode",
                  Justification = "The callers of this method ensure getting the converter is trim compatible - i.e. the type is not Nullable<T>.")]
    private static SmartConverter GetConverter(
        CommandValueLookup lookup,
        CommandValueBinder binder,
        IServiceProvider provider,
        CommandParameter parameter)
    {
        if (parameter.Converter == null)
        {
            if (parameter.PropertyType.IsArray)
            {
                // Return a converter for each array item (not the whole array)
                var elementType = parameter.PropertyType.GetElementType();
                if (elementType == null)
                {
                    throw new InvalidOperationException("Could not get element type");
                }

                return new SmartConverter(TypeDescriptor.GetConverter(elementType), elementType);
            }

            if (parameter.IsFlagValue())
            {
                // Is the optional value instantiated?
                var value = lookup.GetValue(parameter) as IFlagValue;
                if (value == null)
                {
                    // Try to assign it with a null value.
                    // This will create the optional value instance without a value.
                    binder.Bind(parameter, provider, null);
                    value = lookup.GetValue(parameter) as IFlagValue;
                    if (value == null)
                    {
                        throw new InvalidOperationException("Could not initialize optional value.");
                    }
                }

                // Return a converter for the flag element type.
                return new SmartConverter(TypeDescriptor.GetConverter(value.Type), value.Type);
            }

            // TODO Maybe cache conversions?
            return new SmartConverter(TypeDescriptor.GetConverter(parameter.PropertyType), parameter.PropertyType);
        }

        var type = Type.GetType(parameter.Converter.ConverterTypeName);
        if (type == null || provider.GetService(type) is not TypeConverter typeConverter)
        {
            throw CommandRuntimeException.NoConverterFound(parameter);
        }

        return new SmartConverter(typeConverter, type);
    }

    /// <summary>
    /// Convert inputs using the given <see cref="TypeConverter"/> and fallback to finding a constructor taking a single argument of the input type.
    /// </summary>
    private readonly ref struct SmartConverter
    {
        public SmartConverter(
            TypeConverter typeConverter,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            Type type)
        {
            TypeConverter = typeConverter;
            Type = type;
        }

        public TypeConverter TypeConverter { get; }
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        private Type Type { get; }

        public object? ConvertFrom(object input)
        {
            try
            {
                return TypeConverter.ConvertFrom(null, CultureInfo.InvariantCulture, input);
            }
            catch (NotSupportedException)
            {
                var constructor = Type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { input.GetType() }, null);
                if (constructor == null)
                {
                    throw;
                }

                return constructor.Invoke(new[] { input });
            }
        }
    }
}