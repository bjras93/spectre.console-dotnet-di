namespace Spectre.Console.Cli;

internal static class ConfigurationHelper
{
    public static Type? GetSettingsType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TCommand>()
    {
        var commandType = typeof(TCommand);
        if (typeof(ICommand).GetTypeInfo().IsAssignableFrom(commandType) &&
            GetGenericTypeArguments(commandType, typeof(ICommand<>), out var result))
        {
            return result[0];
        }

        return null;
    }

    private static bool GetGenericTypeArguments(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
        Type? type,
        Type genericType,
        [NotNullWhen(true)] out Type[]? genericTypeArguments)
    {
        while (type != null)
        {
            foreach (var @interface in type.GetTypeInfo().GetInterfaces())
            {
                if (!@interface.GetTypeInfo().IsGenericType || @interface.GetGenericTypeDefinition() != genericType)
                {
                    continue;
                }

                genericTypeArguments = @interface.GenericTypeArguments;
                return true;
            }

            type = type.GetTypeInfo().BaseType;
        }

        genericTypeArguments = null;
        return false;
    }
}