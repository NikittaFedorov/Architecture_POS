using POS.Application;
using POS.Application.Reports;

namespace POS.CLI;

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
        ReportTextFormatter.Write(report, output);
    }
}

internal sealed class ExportReportCommand(
    AssessmentSession session, IEnumerable<IReportExporter> exporters)
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
