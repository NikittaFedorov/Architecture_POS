using Architecture_1.Application;

namespace Architecture_1.CLI;

internal sealed class ConsoleMode(AssessmentSession session)
{
    public string Prompt
    {
        get
        {
            var scope = session.Scope;
            if (scope.Teacher is not { } teacher)
                return "> ";

            string teacherLabel = $"преподаватель №{teacher.Number} {teacher.Value.Name}";
            if (scope.Student is { } student)
                return $"[{teacherLabel} / студент №{student.Number} {student.Value.Name}] > ";

            return $"[{teacherLabel}] > ";
        }
    }

    public string EvaluationKeySyntax => session.Scope switch
    {
        { StudentNumber: not null } => "<критерий>",
        { TeacherNumber: not null } => "<студент> <критерий>",
        _ => "<преподаватель> <студент> <критерий>"
    };

    public string[] ReadEvaluationArguments(string input, bool includesScore)
    {
        var scope = session.Scope;
        int keyArgumentCount = 1 + (scope.StudentNumber == null ? 1 : 0) + (scope.TeacherNumber == null ? 1 : 0);
        string[] arguments = CommandArguments.ReadArguments(input);
        if (arguments.Length != keyArgumentCount + (includesScore ? 1 : 0))
        {
            string expected = scope switch
            {
                { StudentNumber: not null } => "критерий",
                { TeacherNumber: not null } => "студента и критерий",
                _ => "преподавателя, студента и критерий"
            };
            string scoreDescription = includesScore ? ", затем балл" : "";
            throw new FormatException($"Укажите {expected}{scoreDescription}.");
        }

        return arguments;
    }
}
