global using ConsoleCommand = Architecture_1.Application.AssessmentSession.ConsoleCommand;

using Architecture_1.Domain;

namespace Architecture_1.Application;

internal sealed partial class AssessmentSession
{
    internal abstract class ConsoleCommand
    {
        public abstract string Name { get; }
        public virtual string Arguments => "";
        public abstract string Description { get; }

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

        protected static IReadOnlyList<Student> GetStudents(AssessmentSession session) => session.Students;
        protected static IReadOnlyList<Teacher> GetTeachers(AssessmentSession session) => session.Teachers;
        protected static IReadOnlyList<Criterion> GetCriteria(AssessmentSession session) => session.Criteria;

        protected static IReadOnlyList<EvaluationRow> GetScores(AssessmentSession session, int student) =>
            session.GetScores(student);
    }
}
