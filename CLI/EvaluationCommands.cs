using Architecture_1.Application;

namespace Architecture_1.CLI;

internal sealed class SetScoreConsoleCommand(AssessmentSession session) : ConsoleCommand
{
    public override string Name => "set-score";
    public override string Arguments => "<номер студента> <номер преподавателя> <номер критерия> <балл>";
    public override string Description => "Выставить оценку по критерию.";

    public override void Execute(string input, TextWriter output)
    {
        string[] words = CommandArguments.ReadWords(input);
        if (words.Length != 4)
            throw new FormatException("Укажите номера студента, преподавателя и критерия, затем балл.");

        int student = CommandArguments.ReadNumber(words[0]);
        int teacher = CommandArguments.ReadNumber(words[1]);
        int criterion = CommandArguments.ReadNumber(words[2]);
        decimal score = CommandArguments.ReadDecimal(words[3]);

        SetScore(session, new SetScoreCommand(student, teacher, criterion, score));
        output.WriteLine("Оценка выставлена.");
    }
}

internal sealed class ListScoresCommand(AssessmentSession session) : ConsoleCommand
{
    public override string Name => "scores";
    public override string Arguments => "<номер студента>";
    public override string Description => "Показать оценки студента.";

    public override void Execute(string input, TextWriter output)
    {
        string[] words = CommandArguments.ReadWords(input);
        if (words.Length != 1)
            throw new FormatException("Укажите номер студента.");

        var scores = GetScores(session, CommandArguments.ReadNumber(words[0]));
        if (scores.Count == 0)
            output.WriteLine("Оценок пока нет.");

        foreach (var row in scores)
            output.WriteLine(FormattableString.Invariant(
                $"№{row.TeacherNumber} {row.Teacher} | №{row.CriterionNumber} {row.Criterion} | {row.Score.Value}"));
    }
}
