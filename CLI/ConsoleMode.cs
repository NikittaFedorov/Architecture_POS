namespace Architecture_1.CLI;

internal class ConsoleMode
{
    public virtual ConsoleMode? Parent => null;
    public virtual int? TeacherNumber => null;
    public virtual int? StudentNumber => null;
    public virtual string Prompt => "> ";
    public virtual string EvaluationKeySyntax => "<преподаватель> <студент> <критерий>";

    public int EvaluationKeyArgumentCount => 1 + (StudentNumber == null ? 1 : 0) + (TeacherNumber == null ? 1 : 0);
    protected virtual string EvaluationKeyDescription => "преподавателя, студента и критерий";

    public string[] ReadEvaluationArguments(string input, bool includesScore)
    {
        string[] arguments = CommandArguments.ReadArguments(input);
        if (arguments.Length != EvaluationKeyArgumentCount + (includesScore ? 1 : 0))
        {
            string scoreDescription = includesScore ? ", затем балл" : "";
            throw new FormatException($"Укажите {EvaluationKeyDescription}{scoreDescription}.");
        }

        return arguments;
    }
}

internal sealed class TeacherMode(ConsoleMode parent, int teacher, string name) : ConsoleMode
{
    public override ConsoleMode Parent => parent;
    public int Number => teacher;
    public string Name => name;
    public override int? TeacherNumber => Number;
    public override string Prompt => $"[преподаватель №{teacher} {Name}] > ";
    public override string EvaluationKeySyntax => "<студент> <критерий>";

    public StudentMode OpenStudent(int student, string studentName) => new(this, student, studentName);

    protected override string EvaluationKeyDescription => "студента и критерий";
}

internal sealed class StudentMode(TeacherMode parent, int student, string name) : ConsoleMode
{
    public override ConsoleMode Parent => parent;
    public override int? TeacherNumber => parent.Number;
    public override int? StudentNumber => student;
    public override string Prompt => $"[преподаватель №{parent.Number} {parent.Name} / студент №{student} {name}] > ";
    public override string EvaluationKeySyntax => "<критерий>";

    protected override string EvaluationKeyDescription => "критерий";
}
