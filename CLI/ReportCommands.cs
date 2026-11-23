using System.Globalization;
using Architecture_1.Application;
using Architecture_1.Application.Reports;

namespace Architecture_1.CLI;

internal abstract class ReportConsoleCommand(AssessmentSession session) : ConsoleCommand
{
    protected const string Filters = "[student <студент>] [teacher <преподаватель>] "
        + "[criterion <критерий>] [average <simple|weighted>] [group <student|teacher|criterion>]";

    protected AssessmentReport CreateReport(string[] arguments)
    {
        if (arguments.Length % 2 != 0)
            throw new FormatException($"Синтаксис команды - {Usage}.");

        var builder = session.CreateReportBuilder();

        for (int i = 0; i < arguments.Length; i += 2)
        {
            string value = arguments[i + 1];
            switch (arguments[i])
            {
                case "student":
                    builder.IncludeStudent(FindStudentNumber(session, value));
                    break;
                case "teacher":
                    builder.IncludeTeacher(FindTeacherNumber(session, value));
                    break;
                case "criterion":
                    builder.IncludeCriterion(FindCriterionNumber(session, value));
                    break;
                case "average":
                    builder.UseAverage(value switch
                    {
                        "simple" => AverageMethod.Simple,
                        "weighted" => AverageMethod.Weighted,
                        _ => throw new FormatException("Укажите average simple или average weighted.")
                    });
                    break;
                case "group":
                    builder.GroupBy(value switch
                    {
                        "student" => ReportGrouping.Student,
                        "teacher" => ReportGrouping.Teacher,
                        "criterion" => ReportGrouping.Criterion,
                        _ => throw new FormatException("Укажите group student, group teacher или group criterion.")
                    });
                    break;
                default:
                    throw new FormatException($"Неизвестный фильтр «{arguments[i]}». Синтаксис команды - {Usage}.");
            }
        }

        return builder.Build();
    }
}

internal sealed class ShowReportCommand(AssessmentSession session)
    : ReportConsoleCommand(session)
{
    public override string Name => "report";
    public override string Arguments => Filters;
    public override string Description => "Показать отчёт в пределах текущего режима. Укажите фильтры и группировку.";

    public override void Execute(string input, TextWriter output)
    {
        AssessmentReport report = CreateReport(CommandArguments.ReadArguments(input));
        var students = report.Students.ToDictionary(student => student.Number);
        var teachers = report.Teachers.ToDictionary(teacher => teacher.Number);
        var criteria = report.Rubric.ToDictionary(criterion => criterion.Number);
        bool showsTotal = report.GroupBy is ReportGrouping.Student or ReportGrouping.Teacher;
        string scoreWeightLabel = (report.GroupBy, report.UsesTeacherWeights) switch
        {
            (ReportGrouping.Teacher, _) => "вес студента",
            (_, true) => "вес преподавателя",
            _ => "вес оценки"
        };
        output.WriteLine(report.GroupBy switch
        {
            ReportGrouping.Teacher => "Отчёт по преподавателям",
            ReportGrouping.Criterion => "Отчёт по критериям",
            _ => "Отчёт по студентам"
        });
        output.WriteLine((report.GroupBy, report.UsesTeacherWeights) switch
        {
            (_, true) => "Среднее по каждому критерию учитывает веса преподавателей, выставивших баллы.",
            (ReportGrouping.Teacher, _) => "В среднем по каждому критерию все оценённые студенты имеют одинаковый вес.",
            _ => "В среднем по каждому критерию все баллы имеют одинаковый вес."
        });
        if (showsTotal)
            output.WriteLine("Общий итог учитывает долю от максимума и вес каждого критерия с оценками.");

        foreach (var group in report.Groups)
        {
            output.WriteLine();
            output.WriteLine(report.GroupBy switch
            {
                ReportGrouping.Teacher => $"Преподаватель №{group.Number} {teachers[group.Number].Name}",
                ReportGrouping.Criterion => $"Критерий №{group.Number} {criteria[group.Number].Name}",
                _ => $"Студент №{group.Number} {students[group.Number].Name}"
            });

            string countLabel = report.GroupBy == ReportGrouping.Student ? "преподавателей" : "оценок";
            var scoresByCriterion = group.Scores.ToLookup(score => score.CriterionNumber);
            foreach (var result in group.StatisticsByCriterion)
            {
                var criterion = criteria[result.CriterionNumber];
                string weightInTotal = showsTotal && result.Average.HasValue
                    ? $" | вес критерия {criterion.Weight.ToString(CultureInfo.InvariantCulture)}"
                    : "";
                output.WriteLine($"№{criterion.Number} {criterion.Name} | максимум {Format(criterion.MaxScore)}"
                    + $" | среднее {Format(result.Average)} | мин. {Format(result.LowestScore)}"
                    + $" | макс. {Format(result.HighestScore)} | {countLabel} {result.ScoreCount}{weightInTotal}");

                foreach (var score in scoresByCriterion[result.CriterionNumber])
                {
                    string details = report.GroupBy switch
                    {
                        ReportGrouping.Teacher =>
                            $"Студент №{score.StudentNumber} {students[score.StudentNumber].Name}",
                        ReportGrouping.Criterion =>
                            $"Преподаватель №{score.TeacherNumber} {teachers[score.TeacherNumber].Name}"
                            + $" -> студент №{score.StudentNumber} {students[score.StudentNumber].Name}",
                        _ => $"Преподаватель №{score.TeacherNumber} {teachers[score.TeacherNumber].Name}"
                    };
                    decimal weight = report.UsesTeacherWeights ? teachers[score.TeacherNumber].Weight : 1m;
                    output.WriteLine($"  {details} | балл {Format(score.Score)}"
                        + $" | {scoreWeightLabel} {weight.ToString(CultureInfo.InvariantCulture)}");
                }
            }

            if (group.Total is decimal total)
            {
                var assessedNames = group.StatisticsByCriterion.Where(result => result.ScoreCount > 0)
                    .Select(result => $"«{criteria[result.CriterionNumber].Name}»");
                output.WriteLine($"Итог по критериям {string.Join(", ", assessedNames)} - "
                    + total.ToString("0.####%", CultureInfo.InvariantCulture));
            }
            else if (showsTotal)
            {
                output.WriteLine("Итог - пока нет оценок по выбранным критериям.");
            }
        }
    }

    private static string Format(decimal? value) => value?.ToString("0.####", CultureInfo.InvariantCulture) ?? "-";
}

internal sealed class ExportReportCommand(
    AssessmentSession session, IReadOnlyList<IReportExporter> exporters)
    : ReportConsoleCommand(session)
{
    public override string Name => "export-report";
    public override string Arguments => $"<{string.Join('|', exporters.Select(exporter => exporter.Format))}> <файл> {Filters}";
    public override string Description => "Выгрузить отчёт с фильтрами и группировкой команды report.";

    public override void Execute(string input, TextWriter output)
    {
        string[] arguments = CommandArguments.ReadArguments(input);
        if (arguments.Length < 2)
            throw new FormatException($"Синтаксис команды - {Usage}.");

        IReportExporter exporter = exporters.FirstOrDefault(item =>
            string.Equals(item.Format, arguments[0], StringComparison.OrdinalIgnoreCase))
            ?? throw new FormatException($"Неизвестный формат. Синтаксис команды - {Usage}.");

        string path = Path.GetFullPath(CommandArguments.ReadText(arguments[1]));
        AssessmentReport report = CreateReport(arguments[2..]);
        SaveReport(path, report, exporter);
        output.WriteLine($"Отчёт сохранён: {path}");
    }

    private static void SaveReport(string path, AssessmentReport report, IReportExporter exporter)
    {
        string temporaryPath = Path.Combine(Path.GetDirectoryName(path)!, $".report-{Guid.NewGuid():N}.tmp");
        try
        {
            using (var file = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write))
                exporter.Write(report, file);

            File.Move(temporaryPath, path);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException($"Не удалось сохранить отчёт: {error.Message}", error);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }
}
