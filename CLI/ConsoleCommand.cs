global using ConsoleCommand = Architecture_1.Application.AssessmentSession.ConsoleCommand;

using System.Globalization;
using Architecture_1.CLI;
using Architecture_1.Domain;

namespace Architecture_1.Application;

internal sealed partial class AssessmentSession
{
    internal abstract class ConsoleCommand
    {
        public abstract string Name { get; }
        public virtual string Arguments => "";
        public abstract string Description { get; }
        public virtual bool IsAvailable => true;

        public string Usage => Arguments.Length == 0 ? Name : $"{Name} {Arguments}";

        public abstract void Execute(string input, TextWriter output);

        protected static int AddStudent(AssessmentSession session, string name) =>
            session.AddStudent(name);

        protected static int AddTeacher(AssessmentSession session, string name, decimal weight = 1m) =>
            session.AddTeacher(name, weight);

        protected static int AddCriterion(AssessmentSession session, string name, decimal maximum, decimal weight) =>
            session.AddCriterion(name, maximum, weight);

        protected static void SetScore(AssessmentSession session, SetScoreCommand command) =>
            session.SetScore(command);

        protected static void CorrectScore(
            AssessmentSession session, int student, int teacher, int criterion, decimal score) =>
            session.CorrectScore(student, teacher, criterion, score);

        protected static void CancelScore(AssessmentSession session, int student, int teacher, int criterion) =>
            session.CancelScore(student, teacher, criterion);

        protected static IReadOnlyList<Student> GetStudents(AssessmentSession session) => session.Students;
        protected static IReadOnlyList<Teacher> GetTeachers(AssessmentSession session) => session.Teachers;
        protected static IReadOnlyList<Criterion> GetCriteria(AssessmentSession session) => session.Criteria;

        protected static int FindStudentNumber(AssessmentSession session, string nameOrNumber) =>
            FindNumber(session.Students, nameOrNumber, "студент", student => student.Name);

        protected static int FindTeacherNumber(AssessmentSession session, string nameOrNumber) =>
            FindNumber(session.Teachers, nameOrNumber, "преподаватель", teacher => teacher.Name);

        protected static (int Teacher, int Student, int Criterion) FindEvaluationNumbers(
            AssessmentSession session, ConsoleMode mode, string[] arguments)
        {
            int argumentIndex = 0;
            int? teacher = mode.TeacherNumber;
            int? student = mode.StudentNumber;

            if (teacher == null)
            {
                teacher = FindTeacherNumber(session, arguments[argumentIndex]);
                argumentIndex++;
            }

            if (student == null)
            {
                student = FindStudentNumber(session, arguments[argumentIndex]);
                argumentIndex++;
            }

            int criterion = FindCriterionNumber(session, arguments[argumentIndex]);
            return (teacher.Value, student.Value, criterion);
        }

        private static int FindCriterionNumber(AssessmentSession session, string nameOrNumber) =>
            FindNumber(session.Criteria, nameOrNumber, "критерий", criterion => criterion.Name);

        private static int FindNumber<T>(IReadOnlyList<T> items, string nameOrNumber, string label,
            Func<T, string> getName)
        {
            bool quoted = nameOrNumber.StartsWith('"');
            if (!quoted && int.TryParse(nameOrNumber, NumberStyles.None, CultureInfo.InvariantCulture, out int number))
            {
                Find(items, number, label);
                return number;
            }

            string name = quoted ? nameOrNumber[1..^1].Replace("\"\"", "\"") : nameOrNumber;
            int found = 0;
            for (int index = 0; index < items.Count; index++)
            {
                if (!string.Equals(getName(items[index]), name, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (found != 0)
                    throw new InvalidOperationException($"Несколько совпадений: {label} «{name}». Укажите номер.");

                found = index + 1;
            }

            if (found == 0)
                throw new InvalidOperationException($"Не найден {label} «{name}».");

            return found;
        }

        protected static string GetStudentName(AssessmentSession session, int student) =>
            session.GetStudentName(student);

        protected static string GetTeacherName(AssessmentSession session, int teacher) =>
            session.GetTeacherName(teacher);

        protected static IReadOnlyList<EvaluationRow> GetScores(
            AssessmentSession session, int student, int? teacher) =>
            session.GetScores(student, teacher);
    }
}
