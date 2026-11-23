using Architecture_1.Application;
using Architecture_1.Application.Reports;
using Architecture_1.CLI;
using Architecture_1.Infrastructure;
using Architecture_1.Infrastructure.DuckDb;
using Architecture_1.Infrastructure.Sqlite;

string provider = Environment.GetEnvironmentVariable("ASSESSMENT_DB_PROVIDER")?.Trim().ToLowerInvariant() ?? "sqlite";
string? databasePath = Environment.GetEnvironmentVariable("ASSESSMENT_DB_PATH");
RelationalDatabase database;
try
{
    database = provider switch
    {
        "sqlite" => new SqliteDatabase(databasePath ?? "assessment.db"),
        "duckdb" => new DuckDbDatabase(databasePath ?? "assessment.duckdb"),
        _ => throw new InvalidOperationException("Укажите sqlite или duckdb в ASSESSMENT_DB_PROVIDER.")
    };
}
catch (InvalidOperationException error)
{
    Console.Error.WriteLine(error.Message);
    return 1;
}

using var connection = database;
var session = new AssessmentSession(
    new SqlStudentRepository(database),
    new SqlTeacherRepository(database),
    new SqlRubricRepository(database),
    new SqlEvaluationRepository(database));
var exit = new ExitCommand();
var mode = new ConsoleMode(session);
IReadOnlyList<IReportExporter> reportExporters = [new JsonReportExporter()];
ConsoleCommand[] commands =
[
    new AddStudentCommand(session),
    new ListStudentsCommand(session),
    new AddTeacherCommand(session),
    new ListTeachersCommand(session),
    new AddCriterionCommand(session),
    new ListCriteriaCommand(session),
    new SetScoreConsoleCommand(session, mode),
    new CorrectScoreConsoleCommand(session, mode),
    new CancelScoreConsoleCommand(session, mode),
    new ListScoresCommand(session),
    new ShowReportCommand(session),
    new ExportReportCommand(session, reportExporters),
    new EnterTeacherCommand(session),
    new EnterStudentCommand(session),
    new BackCommand(session),
    exit,
];
var dispatcher = new CommandDispatcher(commands.Append(new HelpCommand(commands)));

Console.WriteLine("ПОС. Введите help для списка команд.");

while (!exit.Requested)
{
    Console.Write(mode.Prompt);
    string? line = Console.ReadLine();
    if (line == null)
        break;

    dispatcher.Dispatch(line, Console.Out);
    Console.WriteLine();
}

return 0;
