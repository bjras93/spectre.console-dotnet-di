using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli.Internal.Configuration;

namespace Spectre.Console.Cli;

/// <summary>
/// The entry point for a command line application with a default command.
/// </summary>
/// <typeparam name="TDefaultCommand">The type of the default command.</typeparam>
#if !NETSTANDARD2_0
[RequiresDynamicCode("Spectre.Console.Cli relies on reflection. Use during trimming and AOT compilation is not supported and may result in unexpected behaviors.")]
#endif
public sealed class CommandApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TDefaultCommand> : ICommandApp
    where TDefaultCommand : class, ICommand
{
    private readonly CommandApp _app;
    private readonly DefaultCommandConfigurator _defaultCommandConfigurator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandApp{TDefaultCommand}"/> class.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public CommandApp(IServiceCollection services)
    {
        _app = new CommandApp(services);
        _defaultCommandConfigurator = _app.SetDefaultCommand<TDefaultCommand>();
    }

    /// <summary>
    /// Configures the command line application.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public void Configure(Action<IConfigurator> configuration)
    {
        _app.Configure(configuration);
    }

    /// <summary>
    /// Sets up dependencies.
    /// </summary>
    /// <param name="args">Arguments from user input.</param>
    public void Setup(
        IEnumerable<string> args)
    {
        _app.Setup(args);
    }

    /// <summary>
    /// Runs the command line application with specified arguments.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>The exit code from the executed command.</returns>
    public int Run(
        IServiceProvider provider,
        IEnumerable<string> args)
    {
        return _app.Run(provider, args);
    }

    /// <summary>
    /// Runs the command line application with specified arguments.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>The exit code from the executed command.</returns>
    public Task<int> RunAsync(
        IServiceProvider provider,
        IEnumerable<string> args)
    {
        return _app.RunAsync(provider, args);
    }

    internal Configurator GetConfigurator()
    {
        return _app.GetConfigurator();
    }

    /// <summary>
    /// Sets the description of the default command.
    /// </summary>
    /// <param name="description">The default command description.</param>
    /// <returns>The same <see cref="CommandApp{TDefaultCommand}"/> instance so that multiple calls can be chained.</returns>
    public CommandApp<TDefaultCommand> WithDescription(string description)
    {
        _defaultCommandConfigurator.WithDescription(description);
        return this;
    }

    /// <summary>
    /// Sets data that will be passed to the command via the <see cref="CommandContext"/>.
    /// </summary>
    /// <param name="data">The data to pass to the default command.</param>
    /// <returns>The same <see cref="CommandApp{TDefaultCommand}"/> instance so that multiple calls can be chained.</returns>
    public CommandApp<TDefaultCommand> WithData(object data)
    {
        _defaultCommandConfigurator.WithData(data);
        return this;
    }
}