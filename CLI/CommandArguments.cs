using System.Globalization;
using System.Text.RegularExpressions;

namespace Architecture_1.CLI;

internal static class CommandArguments
{
    private static readonly Regex ArgumentPattern = new("""\G\s*("(?:""|[^"])*"|[^\s"]+)(?=\s|$)""");

    public static string[] ReadArguments(string input)
    {
        var arguments = new List<string>();
        int parsedLength = 0;

        foreach (Match match in ArgumentPattern.Matches(input))
        {
            arguments.Add(match.Groups[1].Value);
            parsedLength = match.Index + match.Length;
        }

        if (!string.IsNullOrWhiteSpace(input[parsedLength..]))
            throw new FormatException("Заключите имя или название целиком в двойные кавычки.");

        return arguments.ToArray();
    }

    public static string ReadSingleArgument(string input, string description)
    {
        string[] arguments = ReadArguments(input);
        if (arguments.Length != 1)
            throw new FormatException($"Укажите {description}. Имена из нескольких слов заключайте в кавычки.");

        return arguments[0];
    }

    public static string ReadText(string argument) =>
        argument.StartsWith('"') ? argument[1..^1].Replace("\"\"", "\"") : argument;

    public static decimal ReadDecimal(string input)
    {
        const NumberStyles style = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
        return decimal.TryParse(input.Replace(',', '.'), style, CultureInfo.InvariantCulture, out var value)
            ? value
            : throw new FormatException($"Ожидалось число, получено «{input}».");
    }
}
