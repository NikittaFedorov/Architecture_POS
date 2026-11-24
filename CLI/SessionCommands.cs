namespace POS.CLI;

internal sealed class HelpCommand(IEnumerable<ConsoleCommand> commands, ExitCommand exit) : ConsoleCommand
{
    public override string Name => "help";
    public override string Description => "Показать список команд.";

    public override void Execute(string input, TextWriter output)
    {
        foreach (var command in commands.Append(exit).Append(this).Where(command => command.IsAvailable)
                     .OrderBy(command => command.Name, StringComparer.Ordinal))
        {
            output.WriteLine(command.Usage);
            output.WriteLine($"  {command.Description}");
        }

        output.WriteLine("Студента, преподавателя и критерий можно указать именем или номером.");
        output.WriteLine("Имена и названия из нескольких слов заключайте в двойные кавычки.");
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
