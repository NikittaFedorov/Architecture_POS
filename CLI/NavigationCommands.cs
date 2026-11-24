using POS.Application;

namespace POS.CLI;

internal sealed class EnterTeacherCommand(AssessmentSession session) : ConsoleCommand
{
    public override string Name => "teacher";
    public override string Arguments => "<преподаватель>";
    public override string Description => "Открыть режим преподавателя.";
    public override bool IsAvailable => session.AuthScope.CanSelectTeacher;

    public override void Execute(string input, TextWriter output)
    {
        string nameOrNumber = CommandArguments.ReadSingleArgument(input, "преподавателя");
        int number = FindTeacherNumber(session, nameOrNumber);
        var teacher = session.SelectTeacher(number);
        output.WriteLine($"Выбран преподаватель №{number} {teacher.Name}.");
    }
}

internal sealed class EnterStudentCommand(AssessmentSession session) : ConsoleCommand
{
    public override string Name => "student";
    public override string Arguments => "<студент>";
    public override string Description => "Открыть оценки одного студента.";
    public override bool IsAvailable => session.AuthScope.CanSelectStudent;

    public override void Execute(string input, TextWriter output)
    {
        string nameOrNumber = CommandArguments.ReadSingleArgument(input, "студента");
        int number = FindStudentNumber(session, nameOrNumber);
        var student = session.SelectStudent(number);
        output.WriteLine($"Выбран студент №{number} {student.Name}.");
    }
}

internal sealed class BackCommand(AssessmentSession session) : ConsoleCommand
{
    public override string Name => "back";
    public override string Description => "Вернуться на один уровень.";
    public override bool IsAvailable => session.AuthScope.CanGoBack;

    public override void Execute(string input, TextWriter output)
    {
        session.GoBack();
        output.WriteLine("Возврат на один уровень.");
    }
}
