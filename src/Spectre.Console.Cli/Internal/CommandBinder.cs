namespace Spectre.Console.Cli;

internal static class CommandBinder
{
    public static CommandSettings Bind(
        CommandTree? tree,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        Type settingsType,
        IServiceProvider provider)
    {
        var lookup = CommandValueResolver.GetParameterValues(tree, provider);

        // Got a constructor with at least one name corresponding to a settings?
        foreach (var constructor in settingsType.GetConstructors())
        {
            var parameters = constructor.GetParameters();
            if (parameters.Length > 0)
            {
                foreach (var parameter in parameters)
                {
                    if (lookup.HasParameterWithName(parameter?.Name))
                    {
                        // Use constructor injection.
                        return CommandConstructorBinder.CreateSettings(lookup, constructor, provider);
                    }
                }
            }
        }

        return CommandPropertyBinder.CreateSettings(lookup, settingsType, provider);
    }
}