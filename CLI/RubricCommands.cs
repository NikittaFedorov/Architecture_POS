using Architecture_1.Application;

namespace Architecture_1.CLI;

internal sealed class AddCriterionCommand(AssessmentSession session) : ConsoleCommand
{
    public override string Name => "add-criterion";
    public override string Arguments => "<название> | <максимум> | <вес>";
    public override string Description => "Добавить критерий в рубрикатор.";

    public override void Execute(string input, TextWriter output)
    {
        string[] fields = input.Split('|', StringSplitOptions.TrimEntries);
        if (fields.Length != 3)
            throw new FormatException("Укажите название, максимум и вес через |.");

        decimal maximum = CommandArguments.ReadDecimal(fields[1]);
        decimal weight = CommandArguments.ReadDecimal(fields[2]);
        int number = AddCriterion(session, fields[0], maximum, weight);
        output.WriteLine($"Критерий №{number}.");
    }
}

internal sealed class ListCriteriaCommand(AssessmentSession session) : ConsoleCommand
{
    public override string Name => "criteria";
    public override string Description => "Показать рубрикатор.";

    public override void Execute(string input, TextWriter output)
    {
        ConsoleLists.Write(output, GetCriteria(session).Select(criterion =>
            FormattableString.Invariant(
                $"{criterion.Name} | максимум {criterion.MaxScore} | вес {criterion.Weight}")));
    }
}
