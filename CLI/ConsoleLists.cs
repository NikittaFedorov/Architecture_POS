using Architecture_1.Application;

namespace Architecture_1.CLI;

internal static class ConsoleLists
{
    public static void Write<T>(TextWriter output, IReadOnlyList<Numbered<T>> rows, Func<T, string> format)
    {
        if (rows.Count == 0)
            output.WriteLine("Список пуст.");

        foreach (var row in rows)
            output.WriteLine($"{row.Number} | {format(row.Value)}");
    }
}
