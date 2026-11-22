using Architecture_1.Application;

namespace Architecture_1.CLI;

internal sealed class SetScoreConsoleCommand(AssessmentSession session, ConsoleNavigation navigation) : ConsoleCommand
{
    public override string Name => "set-score";
    public override string Arguments => $"{navigation.Mode.EvaluationKeySyntax} <балл>";
    public override string Description => "Выставить оценку по критерию.";

    public override void Execute(string input, TextWriter output)
    {
        var mode = navigation.Mode;
        string[] arguments = mode.ReadEvaluationArguments(input, includesScore: true);
        decimal score = CommandArguments.ReadDecimal(arguments[^1]);
        var (teacher, student, criterion) = FindEvaluationNumbers(session, mode, arguments);

        SetScore(session, new SetScoreCommand(student, teacher, criterion, score));
        output.WriteLine("Оценка выставлена.");
    }
}

internal sealed class CorrectScoreConsoleCommand(AssessmentSession session, ConsoleNavigation navigation) : ConsoleCommand
{
    public override string Name => "correct-score";
    public override string Arguments => $"{navigation.Mode.EvaluationKeySyntax} <балл>";
    public override string Description => "Исправить выставленную оценку.";

    public override void Execute(string input, TextWriter output)
    {
        var mode = navigation.Mode;
        string[] arguments = mode.ReadEvaluationArguments(input, includesScore: true);
        decimal score = CommandArguments.ReadDecimal(arguments[^1]);
        var (teacher, student, criterion) = FindEvaluationNumbers(session, mode, arguments);

        CorrectScore(session, student, teacher, criterion, score);
        output.WriteLine("Оценка исправлена.");
    }
}

internal sealed class CancelScoreConsoleCommand(AssessmentSession session, ConsoleNavigation navigation) : ConsoleCommand
{
    public override string Name => "cancel-score";
    public override string Arguments => navigation.Mode.EvaluationKeySyntax;
    public override string Description => "Отменить выставленную оценку.";

    public override void Execute(string input, TextWriter output)
    {
        var mode = navigation.Mode;
        string[] arguments = mode.ReadEvaluationArguments(input, includesScore: false);
        var (teacher, student, criterion) = FindEvaluationNumbers(session, mode, arguments);

        CancelScore(session, student, teacher, criterion);
        output.WriteLine("Оценка отменена.");
    }
}

internal sealed class ListScoresCommand(AssessmentSession session, ConsoleNavigation navigation) : ConsoleCommand
{
    public override string Name => "scores";
    public override string Arguments => navigation.Mode.StudentNumber == null ? "<студент>" : "";
    public override string Description => "Показать оценки студента.";

    public override void Execute(string input, TextWriter output)
    {
        var mode = navigation.Mode;
        int student = mode.StudentNumber ?? FindStudentNumber(session, CommandArguments.ReadSingleArgument(input, "студента"));
        var scores = GetScores(session, student, mode.TeacherNumber);
        if (scores.Count == 0)
            output.WriteLine("Оценок пока нет.");

        foreach (var row in scores)
            output.WriteLine(FormattableString.Invariant(
                $"№{row.TeacherNumber} {row.Teacher} | №{row.CriterionNumber} {row.Criterion} | {row.Score.Value}"));
    }
}
