namespace Spectre.Console.Cli;

/// <summary>
/// Represents a command line application.
/// </summary>
public interface ICommandApp
{
    /// <summary>
    /// Configures the command line application.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    void Configure(Action<IConfigurator> configuration);

    /// <summary>
    /// Sets up dependencies.
    /// </summary>
    /// <param name="args">Arguments from user input.</param>
    public void Setup(
        IEnumerable<string> args);

    /// <summary>
    /// Runs the command line application with specified arguments.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>The exit code from the executed command.</returns>
    int Run(
        IServiceProvider provider,
        IEnumerable<string> args);

    /// <summary>
    /// Runs the command line application with specified arguments.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>The exit code from the executed command.</returns>
    Task<int> RunAsync(
        IServiceProvider provider,
        IEnumerable<string> args);
}