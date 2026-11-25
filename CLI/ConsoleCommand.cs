global using ConsoleCommand = POS.Application.AssessmentSession.ConsoleCommand;

using System.Globalization;
using POS.CLI;
using POS.Domain;

namespace POS.Application;

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

        protected static Score CorrectScore(
            AssessmentSession session, int student, int teacher, int criterion, decimal score) =>
            session.CorrectScore(student, teacher, criterion, score);

        protected static Score CancelScore(AssessmentSession session, int student, int teacher, int criterion) =>
            session.CancelScore(student, teacher, criterion);

        protected static string DescribeEvaluation(
            AssessmentSession session, int studentNumber, int teacherNumber, int criterionNumber)
        {
            var student = session.FindStudent(studentNumber);
            var teacher = session.FindTeacher(teacherNumber);
            var criterion = session.FindCriterion(criterionNumber);
            return FormattableString.Invariant(
                $"студент №{studentNumber} {student.Name} | преподаватель №{teacherNumber} {teacher.Name} | критерий №{criterionNumber} {criterion.Name} | максимум {criterion.MaxScore}");
        }

        protected static IReadOnlyList<Numbered<Student>> GetStudents(AssessmentSession session) => session.Students;
        protected static IReadOnlyList<Numbered<Teacher>> GetTeachers(AssessmentSession session) => session.Teachers;
        protected static IReadOnlyList<Numbered<Criterion>> GetCriteria(AssessmentSession session) => session.Criteria;

        protected static int FindStudentNumber(AssessmentSession session, string nameOrNumber) =>
            FindNumber(session.Students, nameOrNumber, "студент", student => student.Name);

        protected static int FindTeacherNumber(AssessmentSession session, string nameOrNumber) =>
            FindNumber(session.Teachers, nameOrNumber, "преподаватель", teacher => teacher.Name);

        protected static (int Teacher, int Student, int Criterion) FindEvaluationNumbers(
            AssessmentSession session, string[] arguments)
        {
            int argumentIndex = 0;
            int? teacher = session.AuthScope.TeacherNumber;
            int? student = session.AuthScope.StudentNumber;

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

        protected static int FindCriterionNumber(AssessmentSession session, string nameOrNumber) =>
            FindNumber(session.Criteria, nameOrNumber, "критерий", criterion => criterion.Name);

        private static int FindNumber<T>(IReadOnlyList<Numbered<T>> items, string nameOrNumber, string label,
            Func<T, string> getName)
        {
            bool quoted = nameOrNumber.StartsWith('"');
            if (!quoted && int.TryParse(nameOrNumber, NumberStyles.None, CultureInfo.InvariantCulture, out int number))
            {
                if (!items.Any(item => item.Number == number))
                    throw new InvalidOperationException($"Не найден {label} №{number}.");
                return number;
            }

            string name = CommandArguments.ReadText(nameOrNumber);
            int found = 0;
            foreach (var item in items)
            {
                if (!string.Equals(getName(item.Value), name, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (found != 0)
                    throw new InvalidOperationException($"Несколько совпадений: {label} «{name}». Укажите номер.");

                found = item.Number;
            }

            if (found == 0)
                throw new InvalidOperationException($"Не найден {label} «{name}».");

            return found;
        }

        protected static IReadOnlyList<EvaluationRow> GetScores(AssessmentSession session, int student) =>
            session.GetScores(student);
    }
}
