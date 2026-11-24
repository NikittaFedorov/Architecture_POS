namespace POS.CLI;

internal sealed class ConsoleLoop(ConsoleMode mode, CommandDispatcher dispatcher, ExitCommand exit)
{
    public void Run(TextReader input, TextWriter output)
    {
        output.WriteLine("ПОС. Введите help для списка команд.");

        while (!exit.Requested)
        {
            output.Write(mode.Prompt);
            string? line = input.ReadLine();
            if (line == null)
                break;

            dispatcher.Dispatch(line, output);
            output.WriteLine();
        }
    }
}
