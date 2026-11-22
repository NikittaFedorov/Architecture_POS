using Architecture_1.Application;
using Architecture_1.CLI;

var session = new AssessmentSession();
var exit = new ExitCommand();
ConsoleCommand[] commands =
[
    new AddStudentCommand(session),
    new ListStudentsCommand(session),
    new AddTeacherCommand(session),
    new ListTeachersCommand(session),
    new AddCriterionCommand(session),
    new ListCriteriaCommand(session),
    new SetScoreConsoleCommand(session),
    new ListScoresCommand(session),
    exit,
];
var dispatcher = new CommandDispatcher(commands.Append(new HelpCommand(commands)));

Console.WriteLine("ПОС. Введите help для списка команд.");

while (!exit.Requested)
{
    Console.Write("> ");
    string? line = Console.ReadLine();
    if (line == null)
        break;

    dispatcher.Dispatch(line, Console.Out);
    Console.WriteLine();
}
