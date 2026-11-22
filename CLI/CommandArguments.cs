using System.Globalization;

namespace Architecture_1.CLI;

internal static class CommandArguments
{
    public static string[] ReadWords(string input) =>
        input.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

    public static int ReadNumber(string input) =>
        int.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out var number)
            ? number
            : throw new FormatException($"Ожидался целочисленный номер, получено «{input}».");

    public static decimal ReadDecimal(string input)
    {
        const NumberStyles style = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
        return decimal.TryParse(input.Replace(',', '.'), style, CultureInfo.InvariantCulture, out var value)
            ? value
            : throw new FormatException($"Ожидалось число, получено «{input}».");
    }
}
