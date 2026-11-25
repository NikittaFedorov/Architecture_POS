using POS.Application;

namespace POS.CLI;

internal sealed class AddStudentCommand(AssessmentSession session) : ConsoleCommand
{
    public override string Name => "add-student";
    public override string Arguments => "<имя>";
    public override string Description => "Зарегистрировать студента.";

    public override void Execute(string input, TextWriter output)
    {
        int number = AddStudent(session, input);
        output.WriteLine($"Добавлен студент №{number}: {input}.");
    }
}

internal sealed class ListStudentsCommand(AssessmentSession session) : ConsoleCommand
{
    public override string Name => "students";
    public override string Description => "Показать студентов.";

    public override void Execute(string input, TextWriter output)
    {
        ConsoleLists.Write(output, GetStudents(session), student => student.Name);
    }
}

internal sealed class AddTeacherCommand(AssessmentSession session) : ConsoleCommand
{
    public override string Name => "add-teacher";
    public override string Arguments => "<имя> [| <вес>]";
    public override string Description => "Зарегистрировать преподавателя.";

    public override void Execute(string input, TextWriter output)
    {
        string[] fields = input.Split('|', StringSplitOptions.TrimEntries);
        if (fields.Length > 2)
            throw new FormatException("Укажите имя и, при необходимости, вес через |.");

        decimal weight = fields.Length == 1 ? 1m : CommandArguments.ReadDecimal(fields[1]);
        int number = AddTeacher(session, fields[0], weight);
        output.WriteLine(FormattableString.Invariant(
            $"Добавлен преподаватель №{number}: {fields[0]} | вес {weight}."));
    }
}

internal sealed class ListTeachersCommand(AssessmentSession session) : ConsoleCommand
{
    public override string Name => "teachers";
    public override string Description => "Показать преподавателей и их веса.";

    public override void Execute(string input, TextWriter output)
    {
        ConsoleLists.Write(output, GetTeachers(session), teacher =>
            FormattableString.Invariant($"{teacher.Name} | вес {teacher.Weight}"));
    }
}
