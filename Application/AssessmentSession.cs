using Architecture_1.Domain;

namespace Architecture_1.Application;

internal sealed partial class AssessmentSession(
    IStudentRepository students,
    ITeacherRepository teachers,
    IRubricRepository rubric,
    IEvaluationRepository evaluations)
{
    private IReadOnlyList<Numbered<Student>> Students => students.List();
    private IReadOnlyList<Numbered<Teacher>> Teachers => teachers.List();
    private IReadOnlyList<Numbered<Criterion>> Criteria => rubric.List();

    private int AddStudent(string name) => students.Add(new Student(Guid.NewGuid(), name));

    private int AddTeacher(string name, decimal weight = 1m) =>
        teachers.Add(new Teacher(Guid.NewGuid(), name, weight));

    private int AddCriterion(string name, decimal maxScore, decimal weight) =>
        rubric.Add(new Criterion(Guid.NewGuid(), name, maxScore, weight));

    private void SetScore(SetScoreCommand command)
    {
        var key = new EvaluationKey(command.Student, command.Teacher, command.Criterion);
        var evaluation = CreateEvaluation(key, command.Score);
        if (!evaluations.TryAdd(key, evaluation.Score))
            throw new InvalidOperationException("Оценка по этой тройке уже выставлена.");
    }

    private void CorrectScore(int student, int teacher, int criterion, decimal score)
    {
        var key = new EvaluationKey(student, teacher, criterion);
        if (evaluations.FindScore(key) == null)
            throw new InvalidOperationException("Оценка по этой тройке не найдена.");

        var evaluation = CreateEvaluation(key, score);

        if (!evaluations.Update(key, evaluation.Score))
            throw new InvalidOperationException("Оценка по этой тройке не найдена.");
    }

    private void CancelScore(int student, int teacher, int criterion)
    {
        if (!evaluations.Remove(new EvaluationKey(student, teacher, criterion)))
            throw new InvalidOperationException("Оценка по этой тройке не найдена.");
    }

    private Evaluation CreateEvaluation(EvaluationKey key, decimal score) =>
        new(FindStudent(key.Student), FindTeacher(key.Teacher), FindCriterion(key.Criterion), new Score(score));

    private Student FindStudent(int number) =>
        students.FindByNumber(number) ?? throw new InvalidOperationException($"Не найден студент №{number}.");

    private Teacher FindTeacher(int number) =>
        teachers.FindByNumber(number) ?? throw new InvalidOperationException($"Не найден преподаватель №{number}.");

    private Criterion FindCriterion(int number) =>
        rubric.FindByNumber(number) ?? throw new InvalidOperationException($"Не найден критерий №{number}.");

    private string GetStudentName(int student) => FindStudent(student).Name;

    private string GetTeacherName(int teacher) => FindTeacher(teacher).Name;

    private IReadOnlyList<EvaluationRow> GetScores(int studentNumber, int? teacherNumber)
    {
        FindStudent(studentNumber);
        return evaluations.ListScoresForStudent(studentNumber, teacherNumber);
    }
}

internal readonly record struct SetScoreCommand(int Student, int Teacher, int Criterion, decimal Score);
