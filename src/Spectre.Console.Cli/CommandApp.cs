using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli.Internal.Configuration;

namespace Spectre.Console.Cli;

/// <summary>
/// The entry point for a command line application.
/// </summary>
public sealed class CommandApp : ICommandApp
{
    private readonly Configurator _configurator;
    private readonly CommandExecutor _executor;
    private bool _executed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandApp"/> class.
    /// </summary>
    /// <param name="services">The registrar.</param>
    public CommandApp(IServiceCollection services)
    {
        _configurator = new Configurator(services);
        _executor = new CommandExecutor(services);
    }

    /// <summary>
    /// Configures the command line application.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public void Configure(Action<IConfigurator> configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        configuration(_configurator);
    }

    /// <summary>
    /// Sets the default command.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <returns>A <see cref="DefaultCommandConfigurator"/> that can be used to configure the default command.</returns>
    public DefaultCommandConfigurator SetDefaultCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TCommand>()
        where TCommand : class, ICommand
    {
        return new DefaultCommandConfigurator(GetConfigurator().SetDefaultCommand<TCommand>());
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
        return RunAsync(provider, args).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Sets up dependencies.
    /// </summary>
    /// <param name="args">Arguments from user input.</param>
    public void Setup(
        IEnumerable<string> args)
    {
        // Add built-in (hidden) commands.
        _configurator.AddBranch(CliConstants.Commands.Branch, cli =>
        {
            cli.HideBranch();
            cli.AddCommand<VersionCommand>(CliConstants.Commands.Version);
            cli.AddCommand<XmlDocCommand>(CliConstants.Commands.XmlDoc);
            cli.AddCommand<ExplainCommand>(CliConstants.Commands.Explain);
        });
        _executor.Setup(_configurator, args);
    }

    /// <summary>
    /// Runs the command line application with specified arguments.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>The exit code from the executed command.</returns>
    public async Task<int> RunAsync(
        IServiceProvider provider,
        IEnumerable<string> args)
    {
        try
        {
            if (!_executed)
            {
                _executed = true;
            }

            return await _executor
                .Execute(_configurator, provider, args)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Should we always propagate when debugging?
            if (Debugger.IsAttached
                && ex is CommandAppException appException
                && appException.AlwaysPropagateWhenDebugging)
            {
                throw;
            }

            if (_configurator.Settings.PropagateExceptions)
            {
                throw;
            }

            if (_configurator.Settings.ExceptionHandler != null)
            {
                return _configurator.Settings.ExceptionHandler(ex, null);
            }

            // Render the exception.
            var pretty = GetRenderableErrorMessage(ex);
            if (pretty != null)
            {
                _configurator.Settings.Console.SafeRender(pretty);
            }

            return -1;
        }
    }

    internal Configurator GetConfigurator()
    {
        return _configurator;
    }

    private static List<IRenderable?>? GetRenderableErrorMessage(Exception ex, bool convert = true)
    {
        if (ex is CommandAppException renderable && renderable.Pretty != null)
        {
            return new List<IRenderable?> { renderable.Pretty };
        }

        if (convert)
        {
            var converted = new List<IRenderable?>
                {
                    new Composer()
                        .Text("[red]Error:[/]")
                        .Space()
                        .Text(ex.Message.EscapeMarkup())
                        .LineBreak(),
                };

            // Got a renderable inner exception?
            if (ex.InnerException != null)
            {
                var innerRenderable = GetRenderableErrorMessage(ex.InnerException, convert: false);
                if (innerRenderable != null)
                {
                    converted.AddRange(innerRenderable);
                }
            }

            return converted;
        }

        return null;
    }
}