namespace Spectre.Console.Cli;

internal sealed class DefaultTypeResolver : IDisposable, ITypeResolver
{
    public ComponentRegistry Registry { get; }

    public DefaultTypeResolver()
        : this(null)
    {
    }

    public DefaultTypeResolver(ComponentRegistry? registry)
    {
        Registry = registry ?? new ComponentRegistry();
    }

    public void Dispose()
    {
        Registry.Dispose();
    }

    public object? Resolve(
        Type? type)
    {
        if (type == null)
        {
            return null;
        }

        Type? elementType = null;
        var isEnumerable = false;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            isEnumerable = true;
            elementType = type.GenericTypeArguments[0];
        }

        var registryType = elementType ?? type;
        var registrations = Registry.GetRegistrations(registryType);
        if (registrations != null && isEnumerable)
        {
            var result = Array.CreateInstance(type, registrations.Count);
            for (var index = 0; index < registrations.Count; index++)
            {
                var registration = registrations.ElementAt(index);
                result.SetValue(Resolve(registration), index);
            }

            return result;
        }

        return Resolve(registrations?.LastOrDefault());
    }

    public object? Resolve(ComponentRegistration? registration)
    {
        return registration?.Activator?.Activate(this);
    }
}