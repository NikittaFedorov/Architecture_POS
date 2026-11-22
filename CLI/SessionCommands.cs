namespace Architecture_1.CLI;

internal sealed class HelpCommand(IReadOnlyList<ConsoleCommand> commands) : ConsoleCommand
{
    public override string Name => "help";
    public override string Description => "Показать список команд.";

    public override void Execute(string input, TextWriter output)
    {
        foreach (var command in commands.Append(this).OrderBy(command => command.Name, StringComparer.Ordinal))
        {
            output.WriteLine(command.Usage);
            output.WriteLine($"  {command.Description}");
        }
    }
}

internal sealed class ExitCommand : ConsoleCommand
{
    public override string Name => "exit";
    public override string Description => "Завершить сеанс.";
    public bool Requested { get; private set; }

    public override void Execute(string input, TextWriter output)
    {
        Requested = true;
    }
}
