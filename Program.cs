using Architecture_1.Application;
using Architecture_1.CLI;

var session = new AssessmentSession();
var exit = new ExitCommand();
var navigation = new ConsoleNavigation();
ConsoleCommand[] commands =
[
    new AddStudentCommand(session),
    new ListStudentsCommand(session),
    new AddTeacherCommand(session),
    new ListTeachersCommand(session),
    new AddCriterionCommand(session),
    new ListCriteriaCommand(session),
    new SetScoreConsoleCommand(session, navigation),
    new CorrectScoreConsoleCommand(session, navigation),
    new CancelScoreConsoleCommand(session, navigation),
    new ListScoresCommand(session, navigation),
    new ConsoleNavigation.EnterTeacherCommand(session, navigation),
    new ConsoleNavigation.EnterStudentCommand(session, navigation),
    new ConsoleNavigation.BackCommand(navigation),
    exit,
];
var dispatcher = new CommandDispatcher(commands.Append(new HelpCommand(commands)));

Console.WriteLine("ПОС. Введите help для списка команд.");

while (!exit.Requested)
{
    Console.Write(navigation.Mode.Prompt);
    string? line = Console.ReadLine();
    if (line == null)
        break;

    dispatcher.Dispatch(line, Console.Out);
    Console.WriteLine();
}
