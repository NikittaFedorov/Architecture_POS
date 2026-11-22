namespace Architecture_1.CLI;

internal sealed class CommandDispatcher
{
    private readonly Dictionary<string, ConsoleCommand> _commands;

    public CommandDispatcher(IEnumerable<ConsoleCommand> commands)
    {
        _commands = commands.ToDictionary(command => command.Name, StringComparer.Ordinal);
    }

    public void Dispatch(string line, TextWriter output)
    {
        var parts = line.Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return;

        if (!_commands.TryGetValue(parts[0], out var command))
        {
            output.WriteLine("Ошибка: неизвестная команда. Введите help.");
            return;
        }

        if (!command.IsAvailable)
        {
            output.WriteLine("Ошибка: команда недоступна в этом режиме. Введите help.");
            return;
        }

        var input = parts.Length == 2 ? parts[1].Trim() : "";
        if (command.Arguments.Length == 0 && input.Length > 0)
        {
            output.WriteLine($"Ошибка: синтаксис команды — {command.Usage}.");
            return;
        }

        try
        {
            command.Execute(input, output);
        }
        catch (Exception error) when (error is ArgumentException or FormatException or InvalidOperationException)
        {
            output.WriteLine($"Ошибка: {error.Message}");
        }
    }
}
