using Architecture_1.Application;

namespace Architecture_1.CLI;

internal sealed class ConsoleNavigation
{
    public ConsoleMode Mode { get; private set; } = new ConsoleMode();

    internal sealed class EnterTeacherCommand(AssessmentSession session, ConsoleNavigation navigation) : ConsoleCommand
    {
        public override string Name => "teacher";
        public override string Arguments => "<преподаватель>";
        public override string Description => "Открыть режим преподавателя.";
        public override bool IsAvailable => navigation.Mode.Parent == null;

        public override void Execute(string input, TextWriter output)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("Сначала вернитесь в главный режим.");

            string nameOrNumber = CommandArguments.ReadSingleArgument(input, "преподавателя");
            int number = FindTeacherNumber(session, nameOrNumber);
            string name = GetTeacherName(session, number);

            navigation.Mode = new TeacherMode(navigation.Mode, number, name);
            output.WriteLine($"Выбран преподаватель №{number} {name}.");
        }
    }

    internal sealed class EnterStudentCommand(AssessmentSession session, ConsoleNavigation navigation) : ConsoleCommand
    {
        public override string Name => "student";
        public override string Arguments => "<студент>";
        public override string Description => "Открыть оценки одного студента.";
        public override bool IsAvailable => navigation.Mode is TeacherMode;

        public override void Execute(string input, TextWriter output)
        {
            if (navigation.Mode is not TeacherMode teacherMode)
                throw new InvalidOperationException("Сначала откройте режим преподавателя.");

            string nameOrNumber = CommandArguments.ReadSingleArgument(input, "студента");
            int number = FindStudentNumber(session, nameOrNumber);
            string name = GetStudentName(session, number);

            navigation.Mode = teacherMode.OpenStudent(number, name);
            output.WriteLine($"Выбран студент №{number} {name}.");
        }
    }

    internal sealed class BackCommand(ConsoleNavigation navigation) : ConsoleCommand
    {
        public override string Name => "back";
        public override string Description => "Вернуться на один уровень.";
        public override bool IsAvailable => navigation.Mode.Parent != null;

        public override void Execute(string input, TextWriter output)
        {
            navigation.Mode = navigation.Mode.Parent
                ?? throw new InvalidOperationException("Вы уже в главном режиме.");
            output.WriteLine("Возврат на один уровень.");
        }
    }
}
