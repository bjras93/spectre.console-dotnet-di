using Microsoft.Extensions.DependencyInjection;

namespace Spectre.Console.Cli;

internal static class TypeRegistrarExtensions
{
    public static void AddDependencies(this IServiceCollection services, CommandModel model)
    {
        var stack = new Stack<CommandInfo>();
        model.Commands.ForEach(c => stack.Push(c));

        while (stack.Count > 0)
        {
            var command = stack.Pop();

            if (command.SettingsType == null)
            {
                // TODO: Error message
                throw new InvalidOperationException("Command setting type cannot be null.");
            }

            if (command.SettingsType is { IsAbstract: false, IsClass: true })
            {
                // Register the settings type
                services.AddTransient(command.SettingsType);
            }

            if (command.CommandType != null)
            {
                services.AddTransient(command.CommandType);
            }

            foreach (var parameter in command.Parameters)
            {
                var pairDeconstructor = parameter?.PairDeconstructor?.Type;
                if (pairDeconstructor != null)
                {
                    services.AddTransient(pairDeconstructor, pairDeconstructor);
                }

                var typeConverterTypeName = parameter?.Converter?.ConverterTypeName;
                if (!string.IsNullOrWhiteSpace(typeConverterTypeName))
                {
                    var typeConverterType = Type.GetType(typeConverterTypeName);
                    Debug.Assert(typeConverterType != null, "Could not create type");
                    services.AddTransient(typeConverterType, typeConverterType);
                }
            }

            foreach (var child in command.Children)
            {
                stack.Push(child);
            }
        }
    }
}