using Microsoft.Extensions.DependencyInjection;

namespace Spectre.Console.Cli;

internal sealed class Configurator : IUnsafeConfigurator, IConfigurator, IConfiguration
{
    private readonly IServiceCollection _services;
    public IList<ConfiguredCommand> Commands { get; }
    public CommandAppSettings Settings { get; }
    public ConfiguredCommand? DefaultCommand { get; private set; }
    public IList<string[]> Examples { get; }

    ICommandAppSettings IConfigurator.Settings => Settings;

    public Configurator(IServiceCollection services)
    {
        _services = services;

        Commands = new List<ConfiguredCommand>();
        Settings = new CommandAppSettings(services);
        Examples = new List<string[]>();
    }

    public void SetHelpProvider(IHelpProvider helpProvider)
    {
        // Register the help provider
        _services.AddSingleton(helpProvider);
    }

    public void SetHelpProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
        where T : IHelpProvider
    {
        // Register the help provider
        _services.AddSingleton(typeof(IHelpProvider), typeof(T));
    }

    public void AddExample(params string[] args)
    {
        Examples.Add(args);
    }

    public ConfiguredCommand SetDefaultCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TDefaultCommand>()
        where TDefaultCommand : class, ICommand
    {
        DefaultCommand = ConfiguredCommand.FromType<TDefaultCommand>(
            CliConstants.DefaultCommandName, isDefaultCommand: true);
        return DefaultCommand;
    }

    public ICommandConfigurator AddCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TCommand>(string name)
        where TCommand : class, ICommand
    {
        var command = Commands.AddAndReturn(ConfiguredCommand.FromType<TCommand>(name, isDefaultCommand: false));
        return new CommandConfigurator(command);
    }

    public ICommandConfigurator AddDelegate<TSettings>(string name, Func<CommandContext, TSettings, int> func)
        where TSettings : CommandSettings
    {
        var command = Commands.AddAndReturn(ConfiguredCommand.FromDelegate<TSettings>(
            name, (context, settings) => Task.FromResult(func(context, (TSettings)settings))));
        return new CommandConfigurator(command);
    }

    public ICommandConfigurator AddAsyncDelegate<TSettings>(string name, Func<CommandContext, TSettings, Task<int>> func)
        where TSettings : CommandSettings
    {
        var command = Commands.AddAndReturn(ConfiguredCommand.FromDelegate<TSettings>(
            name, (context, settings) => func(context, (TSettings)settings)));
        return new CommandConfigurator(command);
    }

    public IBranchConfigurator AddBranch<TSettings>(string name, Action<IConfigurator<TSettings>> action)
        where TSettings : CommandSettings
    {
        var command = ConfiguredCommand.FromBranch<TSettings>(name);
        action(new Configurator<TSettings>(command, _services));
        var added = Commands.AddAndReturn(command);
        return new BranchConfigurator(added);
    }

    ICommandConfigurator IUnsafeConfigurator.AddCommand(
        string name,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
        Type command)
    {
        var method = GetType().GetMethod("AddCommand");
        if (method == null)
        {
            throw new CommandConfigurationException("Could not find AddCommand by reflection.");
        }

        method = method.MakeGenericMethod(command);

        if (!(method.Invoke(this, new object[] { name }) is ICommandConfigurator result))
        {
            throw new CommandConfigurationException("Invoking AddCommand returned null.");
        }

        return result;
    }

    IBranchConfigurator IUnsafeConfigurator.AddBranch(string name, Type settings, Action<IUnsafeBranchConfigurator> action)
    {
        var command = ConfiguredCommand.FromBranch(settings, name);

        // Create the configurator.
        var configuratorType = typeof(Configurator<>).MakeGenericType(settings);
        if (!(Activator.CreateInstance(configuratorType, new object?[] { command, _services }) is IUnsafeBranchConfigurator configurator))
        {
            throw new CommandConfigurationException("Could not create configurator by reflection.");
        }

        action(configurator);
        var added = Commands.AddAndReturn(command);
        return new BranchConfigurator(added);
    }
}