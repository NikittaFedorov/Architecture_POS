using Architecture_1.Domain;

namespace Architecture_1.Application;

internal sealed partial class AssessmentSession
{
    private readonly List<Student> _students = [];
    private readonly List<Teacher> _teachers = [];
    private readonly List<Criterion> _criteria = [];
    private readonly Dictionary<(int Student, int Teacher, int Criterion), Evaluation> _evaluations = [];

    private IReadOnlyList<Student> Students => _students.AsReadOnly();
    private IReadOnlyList<Teacher> Teachers => _teachers.AsReadOnly();
    private IReadOnlyList<Criterion> Criteria => _criteria.AsReadOnly();

    private int AddStudent(string name)
    {
        _students.Add(new Student(Guid.NewGuid(), name));
        return _students.Count;
    }

    private int AddTeacher(string name, decimal weight = 1m)
    {
        _teachers.Add(new Teacher(Guid.NewGuid(), name, weight));
        return _teachers.Count;
    }

    private int AddCriterion(string name, decimal maxScore, decimal weight)
    {
        _criteria.Add(new Criterion(Guid.NewGuid(), name, maxScore, weight));
        return _criteria.Count;
    }

    private void SetScore(SetScoreCommand command)
    {
        var student = Find(_students, command.Student, "студент");
        var teacher = Find(_teachers, command.Teacher, "преподаватель");
        var criterion = Find(_criteria, command.Criterion, "критерий");
        var key = (command.Student, command.Teacher, command.Criterion);

        var evaluation = new Evaluation(student, teacher, criterion, new Score(command.Score));
        if (!_evaluations.TryAdd(key, evaluation))
            throw new InvalidOperationException("Оценка по этой тройке уже выставлена.");
    }

    private IReadOnlyList<EvaluationRow> GetScores(int student)
    {
        Find(_students, student, "студент");

        return _evaluations
            .Where(entry => entry.Key.Student == student)
            .Select(entry => new EvaluationRow(
                entry.Key.Teacher, _teachers[entry.Key.Teacher - 1].Name,
                entry.Key.Criterion, _criteria[entry.Key.Criterion - 1].Name,
                entry.Value.Score))
            .ToArray();
    }

    private static T Find<T>(IReadOnlyList<T> items, int number, string label)
    {
        if (number < 1 || number > items.Count)
            throw new InvalidOperationException($"Не найден {label} №{number}.");

        return items[number - 1];
    }
}

internal readonly record struct SetScoreCommand(int Student, int Teacher, int Criterion, decimal Score);

internal readonly record struct EvaluationRow(
    int TeacherNumber, string Teacher, int CriterionNumber, string Criterion, Score Score);
