namespace Spectre.Console.Cli;

internal sealed class ConfiguredCommand
{
    public string Name { get; }
    public HashSet<string> Aliases { get; }
    public string? Description { get; set; }
    public object? Data { get; set; }
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type? CommandType { get; }
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
    public Type SettingsType { get; }
    public Func<CommandContext, CommandSettings, Task<int>>? Delegate { get; }
    public bool IsDefaultCommand { get; }
    public bool IsHidden { get; set; }

    public IList<ConfiguredCommand> Children { get; }
    public IList<string[]> Examples { get; }

    private ConfiguredCommand(
        string name,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces |
    DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)]
        Type? commandType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces |
    DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)]
        Type settingsType,
        Func<CommandContext, CommandSettings, Task<int>>? @delegate,
        bool isDefaultCommand)
    {
        Name = name;
        Aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        CommandType = commandType;
        SettingsType = settingsType;
        Delegate = @delegate;
        IsDefaultCommand = isDefaultCommand;

        // Default commands are always created as hidden.
        IsHidden = IsDefaultCommand;

        Children = new List<ConfiguredCommand>();
        Examples = new List<string[]>();
    }

    public static ConfiguredCommand FromBranch(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)]
        Type settings,
        string name)
    {
        return new ConfiguredCommand(name, null, settings, null, false);
    }

    public static ConfiguredCommand FromBranch<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] TSettings>(string name)
        where TSettings : CommandSettings
    {
        return new ConfiguredCommand(name, null, typeof(TSettings), null, false);
    }

    public static ConfiguredCommand FromType<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces |
    DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] TCommand>(string name, bool isDefaultCommand = false)
            where TCommand : class, ICommand
    {
        var settingsType = ConfigurationHelper.GetSettingsType<TCommand>();
        if (settingsType == null)
        {
            throw CommandRuntimeException.CouldNotGetSettingsType(typeof(TCommand));
        }

#pragma warning disable IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
        return new ConfiguredCommand(name, typeof(TCommand), settingsType, null, isDefaultCommand);
#pragma warning restore IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
    }

    public static ConfiguredCommand FromDelegate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)] TSettings>(
        string name, Func<CommandContext, CommandSettings, Task<int>>? @delegate = null)
        where TSettings : CommandSettings
    {
        return new ConfiguredCommand(name, null, typeof(TSettings), @delegate, false);
    }
}