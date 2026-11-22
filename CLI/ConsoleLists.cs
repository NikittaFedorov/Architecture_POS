namespace Architecture_1.CLI;

internal static class ConsoleLists
{
    public static void Write(TextWriter output, IEnumerable<string> rows)
    {
        int number = 1;
        foreach (string row in rows)
        {
            output.WriteLine($"{number} | {row}");
            number++;
        }

        if (number == 1)
            output.WriteLine("Список пуст.");
    }
}
